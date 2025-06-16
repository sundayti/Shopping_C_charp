namespace OrdersService.Presentation.Middlewares;

public class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
{
    public async Task Invoke(HttpContext ctx)
    {
        logger.LogInformation("Incoming {Method} {Path}", ctx.Request.Method, ctx.Request.Path);
        await next(ctx);
        logger.LogInformation("Outgoing {StatusCode}", ctx.Response.StatusCode);
    }
}