
using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text.Json;
using ZLearn.Application.Common.Model;

namespace ZLearn.Application.Common.Behavours
{
    public class HandleExceptionBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly ILogger<TRequest> _logger;

        public HandleExceptionBehaviour(ILogger<TRequest> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            try
            {
                return await next(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred when handling request: {Message}", ex.Message);
                throw;
            }
        }
    }
}
