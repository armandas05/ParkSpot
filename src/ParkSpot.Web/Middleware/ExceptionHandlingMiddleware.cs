using ParkSpot.Web.Exceptions;
using ParkSpot.Web.Entities.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace ParkSpot.Web.Middleware;

public class ExceptionHandlingMiddleware : IMiddleware
{
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (DomainException exception)
        {
            await WriteError(context, exception.StatusCode, exception.Message);
        }
        catch (BadHttpRequestException)
        {
            await WriteError(context, 400, "Netinkami užklausos duomenys.");
        }
        catch (MySqlException exception) when (exception.Number == 1062)
        {
            await WriteError(context, 409, "Įrašas jau egzistuoja. Atnaujinkite duomenis ir bandykite dar kartą.");
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _logger.LogError(exception, "Request failed");

            await WriteError(context, 500, "Serverio klaida. Bandykite dar kartą.");
        }
    }

    private static Task WriteError(HttpContext context, int status, string message)
    {
        context.Response.StatusCode = status;

        return context.Response.WriteAsJsonAsync(new ErrorResponse(message));
    }
}
