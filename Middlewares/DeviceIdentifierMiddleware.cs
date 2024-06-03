namespace QuickTicketStoreDemo.Middlewares;

public class DeviceIdentifierMiddleware
{
    private readonly RequestDelegate _next;

    public DeviceIdentifierMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public Task Invoke(HttpContext httpContext)
    {

        if (!httpContext.Request.Cookies.ContainsKey("AspNetDeviceToken"))
        {
            var uniqueToken = $"{DateTime.UtcNow:fffffff}{Guid.NewGuid():N}";
            httpContext.Response.Cookies.Append("AspNetDeviceToken", uniqueToken, new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.Strict,
                IsEssential = true,
                Expires = DateTimeOffset.UtcNow.AddYears(3)
            });
        }

        return _next(httpContext);
    }
}

public static class DeviceIdentifierMiddlewareExtensions
{
    public static IApplicationBuilder UseDeviceIdentifierMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<DeviceIdentifierMiddleware>();
    }
}
