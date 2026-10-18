using RepositoryContracts;

namespace WebAPI.Middleware;

public class GlobalExceptionHandlerMiddleware : IMiddleware
{
    private readonly ILogger<GlobalExceptionHandlerMiddleware> logger;

    public GlobalExceptionHandlerMiddleware(ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        this.logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception e)
        {
            int statusCode = e switch
            {
                NotFoundException => StatusCodes.Status404NotFound,
                ArgumentException => StatusCodes.Status400BadRequest,
                InvalidOperationException => StatusCodes.Status409Conflict,
                _ => StatusCodes.Status500InternalServerError
            };

            if (statusCode == StatusCodes.Status500InternalServerError)
            {
                logger.LogError(e, "Unhandled exception");
            }

            string message = statusCode == StatusCodes.Status500InternalServerError
                ? "An unexpected error occurred."
                : e.Message;

            context.Response.StatusCode = statusCode;
            await context.Response.WriteAsJsonAsync(new { status = statusCode, error = message });
        }
    }
}
