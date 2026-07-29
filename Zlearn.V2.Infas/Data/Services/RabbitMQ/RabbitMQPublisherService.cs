using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using Zlearn.V2.Application.Common.Utils;
using Zlearn.V2.Infas.Data.Outbox;

namespace Zlearn.V2.Infas.Messaging
{
    public class RabbitMQPublisherService : IRabbitMQPublisherService, IDisposable
    {
        public const string EXCHANGE_NAME = "zlearn.domain.events";
        private readonly ILogger<RabbitMQPublisherService> _logger;
        private IConnection? _connection;
        private IModel? _channel;
        private readonly object _lock = new object();

        public RabbitMQPublisherService(ILogger<RabbitMQPublisherService> logger)
        {
            _logger = logger;
        }

        private void EnsureConnected()
        {
            if (_connection != null && _connection.IsOpen && _channel != null && _channel.IsOpen)
                return;

            lock (_lock)
            {
                if (_connection != null && _connection.IsOpen && _channel != null && _channel.IsOpen)
                    return;

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

                    // Khởi tạo Topic Exchange
                    _channel.ExchangeDeclare(
                        exchange: EXCHANGE_NAME,
                        type: ExchangeType.Topic,
                        durable: true,
                        autoDelete: false
                    );

                    _logger.LogInformation("RabbitMQ Publisher connected successfully to {Host}:{Port}", host, port);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not connect to RabbitMQ broker on startup. Event publishing will fallback or retry.");
                    _channel = null;
                    _connection = null;
                }
            }
        }

        public Task PublishEventAsync(OutboxEvent outboxEvent)
        {
            EnsureConnected();

            if (_channel == null || !_channel.IsOpen)
            {
                throw new InvalidOperationException("RabbitMQ connection is unavailable.");
            }

            var eventName = GetShortEventName(outboxEvent.Type);
            var routingKey = $"zlearn.event.{eventName}";

            var body = Encoding.UTF8.GetBytes(outboxEvent.Content);

            var props = _channel.CreateBasicProperties();
            props.Persistent = true;
            props.MessageId = outboxEvent.Id.ToString();
            props.Type = outboxEvent.Type;
            props.Timestamp = new AmqpTimestamp(outboxEvent.OccurredOn.ToUnixTimeSeconds());

            lock (_lock)
            {
                _channel.BasicPublish(
                    exchange: EXCHANGE_NAME,
                    routingKey: routingKey,
                    mandatory: false,
                    basicProperties: props,
                    body: body
                );
            }

            _logger.LogInformation("Published Outbox Event {Id} (Type: {Type}) to RabbitMQ Exchange '{Exchange}' with RoutingKey '{RoutingKey}'",
                outboxEvent.Id, outboxEvent.Type, EXCHANGE_NAME, routingKey);

            return Task.CompletedTask;
        }

        private static string GetShortEventName(string fullTypeName)
        {
            var parts = fullTypeName.Split(',');
            var typeOnly = parts[0].Trim();
            var lastDot = typeOnly.LastIndexOf('.');
            return lastDot >= 0 ? typeOnly.Substring(lastDot + 1).ToLowerInvariant() : typeOnly.ToLowerInvariant();
        }

        public void Dispose()
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
                _logger.LogError(ex, "Error disposing RabbitMQ connection.");
            }
        }
    }
}
