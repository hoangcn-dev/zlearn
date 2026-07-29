using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Zlearn.V2.Application.Common.Utils;

namespace Zlearn.V2.Infas.Messaging
{
    public class RabbitMQConsumerBackgroundService : BackgroundService
    {
        public const string QUEUE_NAME = "zlearn.projections.mongo";
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<RabbitMQConsumerBackgroundService> _logger;
        private IConnection? _connection;
        private IModel? _channel;

        public RabbitMQConsumerBackgroundService(IServiceProvider serviceProvider, ILogger<RabbitMQConsumerBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("RabbitMQ Consumer Background Service is starting.");

            await Task.Yield();

            try
            {
                var host = EnvVariableHelper.GetValueOrDefault(EnvVariableNames.RABBITMQ_HOST, "localhost");
                var portStr = EnvVariableHelper.GetValueOrDefault(EnvVariableNames.RABBITMQ_PORT, "5672");
                int.TryParse(portStr, out var port);
                if (port <= 0) port = 5672;

                var username = EnvVariableHelper.GetValueOrDefault(EnvVariableNames.RABBITMQ_USERNAME, "guest");
                var password = EnvVariableHelper.GetValueOrDefault(EnvVariableNames.RABBITMQ_PASSWORD, "guest");

                var factory = new ConnectionFactory
                {
                    HostName = host,
                    Port = port,
                    UserName = username,
                    Password = password,
                    DispatchConsumersAsync = true
                };

                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                _channel.ExchangeDeclare(
                    exchange: RabbitMQPublisherService.EXCHANGE_NAME,
                    type: ExchangeType.Topic,
                    durable: true,
                    autoDelete: false
                );

                _channel.QueueDeclare(
                    queue: QUEUE_NAME,
                    durable: true,
                    exclusive: false,
                    autoDelete: false
                );

                _channel.QueueBind(
                    queue: QUEUE_NAME,
                    exchange: RabbitMQPublisherService.EXCHANGE_NAME,
                    routingKey: "zlearn.event.#"
                );

                _channel.BasicQos(prefetchSize: 0, prefetchCount: 10, global: false);

                var consumer = new AsyncEventingBasicConsumer(_channel);
                consumer.Received += async (model, ea) =>
                {
                    await ProcessMessageAsync(ea);
                };

                _channel.BasicConsume(queue: QUEUE_NAME, autoAck: false, consumer: consumer);

                _logger.LogInformation("RabbitMQ Consumer subscribed to Queue '{Queue}' successfully.", QUEUE_NAME);

                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("RabbitMQ Consumer is shutting down.");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "RabbitMQ Consumer encountered an error on connection/subscribe.");
            }
        }

        private async Task ProcessMessageAsync(BasicDeliverEventArgs ea)
        {
            if (_channel == null) return;

            try
            {
                var body = ea.Body.ToArray();
                var jsonContent = Encoding.UTF8.GetString(body);
                var typeName = ea.BasicProperties.Type;

                if (string.IsNullOrEmpty(typeName))
                {
                    _logger.LogWarning("Received RabbitMQ message without Type header. Rejecting.");
                    _channel.BasicReject(ea.DeliveryTag, requeue: false);
                    return;
                }

                var eventType = Type.GetType(typeName);
                if (eventType == null)
                {
                    _logger.LogWarning("Cannot resolve event type '{Type}'. Rejecting message.", typeName);
                    _channel.BasicReject(ea.DeliveryTag, requeue: false);
                    return;
                }

                var domainEvent = JsonConvert.DeserializeObject(jsonContent, eventType) as INotification;
                if (domainEvent == null)
                {
                    _logger.LogWarning("Failed to deserialize event content for type '{Type}'. Rejecting message.", typeName);
                    _channel.BasicReject(ea.DeliveryTag, requeue: false);
                    return;
                }

                using var scope = _serviceProvider.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                await mediator.Publish(domainEvent);

                _channel.BasicAck(ea.DeliveryTag, multiple: false);

                _logger.LogInformation("RabbitMQ Consumer processed event {Type} successfully.", typeName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing RabbitMQ message. DeliveryTag: {Tag}", ea.DeliveryTag);
                _channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: true);
            }
        }

        public override void Dispose()
        {
            try
            {
                _channel?.Close();
                _channel?.Dispose();
                _connection?.Close();
                _connection?.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disposing RabbitMQ Consumer.");
            }

            base.Dispose();
        }
    }
}
