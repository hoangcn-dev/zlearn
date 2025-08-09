
using FluentValidation;
using System.Text.Json;
using ZLearn.API.Exceptions;
using ZLearn.Application.Common.DTOs;

namespace ZLearn.Web.Middlewares
{
    public class ExceptionMiddleware : IMiddleware
    {
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(ILogger<ExceptionMiddleware> logger)
        {
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var url = context.Request.Path.ToString();
            if (url.StartsWith("/api", StringComparison.OrdinalIgnoreCase))
            {
                // Handle API exceptions
                await HandApiExceptionAsync(context, next);
            }
            else
            {
                // Handle non-API exceptions
                try
                {
                    await next(context);
                }
                catch (UnauthorizedException ex)
                {
                    _logger.LogError(ex, "Authorize failed.");
                    if (!context.Response.HasStarted)
                    {
                        var currentUrl = context.Request.Path + context.Request.QueryString;
                        context.Response.Redirect($"/unauthoried?returnUrl={Uri.EscapeDataString(currentUrl)}");
                    }
                    else
                    {
                        await context.Response.WriteAsync("ALogin to continue.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An unhandled exception occurred while processing the request.");
                    if (!context.Response.HasStarted)
                    {
                        context.Response.Redirect("/error");
                    }
                    else
                    {
                        await context.Response.WriteAsync("An unexpected error occurred.");
                    }
                }
            }
        }

        private async Task HandApiExceptionAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (UnauthorizedException ex)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await WriteResponseAsync(context, Result<NoData>.Failure(ex.Message, ErrorCodes.UNAUTHORIZED));
            }
            catch (TokenExpiredException ex)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await WriteResponseAsync(context, Result<NoData>.Failure(ex.Message, ErrorCodes.TOKEN_EXPIRED));
            }
            catch (InvalidCredentialsException ex)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await WriteResponseAsync(context, Result<NoData>.Failure(ex.Message));
            }
            catch (ForbiddenException ex)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.Redirect("/forbidden");
                //await WriteResponseAsync(context, Result<NoData>.Failure(ex.Message, ErrorCodes.ACCOUNT_LOCKED));
            }
            catch (NotFoundException ex)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                await WriteResponseAsync(context, Result<NoData>.Failure(ex.Message));
            }
            catch (ResourceConflictException ex)
            {
                context.Response.StatusCode = StatusCodes.Status409Conflict;
                await WriteResponseAsync(context, Result<NoData>.Failure(ex.Message));
            }
            catch (DuplicateEntryException ex)
            {
                context.Response.StatusCode = StatusCodes.Status409Conflict;
                await WriteResponseAsync(context, Result<NoData>.Failure(ex.Message));
            }
            catch (ServiceUnavailableException ex)
            {
                context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                await WriteResponseAsync(context, Result<NoData>.Failure(ex.Message));
            }
            catch (DatabaseErrorException ex)
            {
                _logger.LogError(ex, "Database error occurred: {Message}", ex.Message);
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await WriteResponseAsync(context, Result<NoData>.Failure(ex.Message));
            }
            catch (InternalErrorException ex)
            {
                _logger.LogError(ex, "Internal server error occurred: {Message}", ex.Message);
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await WriteResponseAsync(context, Result<NoData>.Failure(ex.Message));
            }
            catch (ValidationException ex)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                var errors = ex.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
                await WriteResponseAsync(context, Result<NoData>.Failure(errors.First().ErrorMessage));
            }
            catch (ArgumentException ex)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await WriteResponseAsync(context, Result<NoData>.Failure(ex.Message));
            }
            catch (Exception ex)
            {
                // Fallback for any non-API exceptions
                _logger.LogError(ex, "An unexpected error occurred: {Message}", ex.Message);
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await WriteResponseAsync(context, Result<NoData>.Failure("An unexpected error occurred"));
            }
        }

        private static async Task WriteResponseAsync(HttpContext context, object response)
        {
            context.Response.ContentType = "application/json";
            var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            await context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
        }
    }

    public static class ExceptionMiddlewareExtensions
    {
        public static void UseExceptionMiddleware(this IApplicationBuilder builder)
        {
            builder.UseMiddleware<ExceptionMiddleware>();
        }

        public static void AddExceptionMiddleware(this IServiceCollection services)
        {
            services.AddTransient<ExceptionMiddleware>();
        }
    }
}
