using System.Net;
using System.Net.Mime;
using Matrix.Data.Exceptions;
using Matrix.Data.Models.Web;
using Matrix.Utilities;
using Newtonsoft.Json;
using WebException = Matrix.Data.Exceptions.WebException;

namespace Matrix.WebServices;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public Task InvokeAsync(HttpContext context)
    {
        return _next.Invoke(context).OnFailure(exception => HandleExceptionAsync(context, exception));
    }

    public Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var rootException = exception.GetBaseException();
        var statusCode = GetHttpStatusCode(rootException);
        
        context.Response.ContentType = MediaTypeNames.Application.Json;
        context.Response.StatusCode = (int) statusCode;

        ExceptionResponse exceptionResponse = new ExceptionResponse()
        {
            StatusCode = (int) statusCode,
            Message = rootException.Message,
            Details = rootException.StackTrace ?? string.Empty
        };
        
        return context.Response.WriteAsync(JsonConvert.SerializeObject(exceptionResponse));
    }

    private HttpStatusCode GetHttpStatusCode(Exception exception)
    {
        switch (exception)
        {
            case MatrixEntityNotFoundException:
            case TimerException:
                return HttpStatusCode.NotFound;
            case ArgumentException: 
            case ClockFaceException:
            case WebException:
                return HttpStatusCode.BadRequest;
            default:
                return HttpStatusCode.InternalServerError;
        }
    }
}