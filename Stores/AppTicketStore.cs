using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using QuickTicketStoreDemo.Data.Operations;
using QuickTicketStoreDemo.Data;
using StackExchange.Redis;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace QuickTicketStoreDemo.Stores;

public class AppTicketStore : ITicketStore
{
    private readonly IServiceCollection services;

    public AppTicketStore(IServiceCollection services)
    {
        this.services = services;
    }
    public async Task RemoveAsync(string key)
    {
        if (Guid.TryParse(key, out _))
        {

            using var scope = services.BuildServiceProvider().CreateScope();

            var cache = scope.ServiceProvider.GetService<IConnectionMultiplexer>()?.GetDatabase()
                ?? throw new InvalidOperationException(nameof(IConnectionMultiplexer));

            var ticketFromCache = await cache.StringGetAsync(key);
            if (ticketFromCache.HasValue)
            {
                var deserializedTicket = JsonSerializer.Deserialize<AppAuthenticationTicket?>(ticketFromCache.ToString());

                if (deserializedTicket is not null)
                {
                    var userId = deserializedTicket.UserId;
                    await cache.SetRemoveAsync($"UserTickets:{userId}", deserializedTicket.Key);
                    await cache.KeyDeleteAsync(deserializedTicket.Key);
                }
            }
        }
    }

    public async Task RenewAsync(string key, AuthenticationTicket ticket)
    {
        if (Guid.TryParse(key, out _))
        {
            using var scope = services.BuildServiceProvider().CreateScope();

            var cache = scope.ServiceProvider.GetService<IConnectionMultiplexer>()?.GetDatabase()
                ?? throw new InvalidOperationException(nameof(IConnectionMultiplexer));

            var ticketFromCache = await cache.StringGetAsync(key);
            if (ticketFromCache.HasValue)
            {
                var deserializedTicket = JsonSerializer.Deserialize<AppAuthenticationTicket?>(ticketFromCache.ToString());

                if (deserializedTicket is not null)
                {
                    var bytes = TicketSerializer.Default.Serialize(ticket);

                    deserializedTicket.Value = Convert.ToBase64String(bytes);
                    deserializedTicket.LastActive = DateTime.UtcNow;
                    deserializedTicket.Expires = ticket.Properties.ExpiresUtc;

                    var deserializedTicketJson = JsonSerializer.Serialize(deserializedTicket);
                    await cache.StringSetAsync(key, deserializedTicketJson, TimeSpan.FromDays(30));
                }
            }
        }
    }

    public async Task<AuthenticationTicket?> RetrieveAsync(string key)
    {
        if (Guid.TryParse(key, out _))
        {
            using var scope = services.BuildServiceProvider().CreateScope();

            var cache = scope.ServiceProvider.GetService<IConnectionMultiplexer>()?.GetDatabase()
                ?? throw new InvalidOperationException(nameof(IConnectionMultiplexer));

            var ticketFromCache = await cache.StringGetAsync(key);
            if (ticketFromCache.HasValue)
            {
                var deserializedTicket = JsonSerializer.Deserialize<AppAuthenticationTicket?>(ticketFromCache.ToString());

                if (deserializedTicket is not null)
                {
                    deserializedTicket.LastActive = DateTime.UtcNow;

                    var deserializedTicketJson = JsonSerializer.Serialize(deserializedTicket);
                    await cache.StringSetAsync(key, deserializedTicketJson, TimeSpan.FromDays(30));

                    var bytes = Convert.FromBase64String(deserializedTicket.Value);

                    return TicketSerializer.Default.Deserialize(bytes);
                }
            }
        }

        return null;
    }

    public async Task<string> StoreAsync(AuthenticationTicket ticket)
    {
        using var scope = services.BuildServiceProvider().CreateScope();

        var context = scope.ServiceProvider.GetService<AppIdentityDbContext>()
            ?? throw new InvalidOperationException(nameof(AppIdentityDbContext));

        var cache = scope.ServiceProvider.GetService<IConnectionMultiplexer>()?.GetDatabase()
            ?? throw new InvalidOperationException(nameof(IConnectionMultiplexer));

        var userId = ticket.Principal.FindFirstValue(ClaimTypes.NameIdentifier);

        if (ticket.AuthenticationScheme == IdentityConstants.ExternalScheme)
        {
            userId = (await context.UserLogins.SingleOrDefaultAsync(l => l.ProviderKey == userId))?.UserId;
        }

        if (string.IsNullOrEmpty(userId))
            return string.Empty;

        var bytes = Convert.ToBase64String(TicketSerializer.Default.Serialize(ticket));

        var appAuthenticationTicket = new AppAuthenticationTicket
        {
            UserId = userId,
            LastActive = DateTimeOffset.UtcNow,
            Value = bytes,
            Expires = ticket.Properties.ExpiresUtc.HasValue ? ticket.Properties.ExpiresUtc.Value : null
        };

        var httpContextAccessor = scope.ServiceProvider.GetService<IHttpContextAccessor>();
        var httpContext = httpContextAccessor?.HttpContext;

        if (httpContext is not null)
        {
            var remoteIpAddress = httpContext.Connection.RemoteIpAddress?.ToString();
            if (!string.IsNullOrEmpty(remoteIpAddress))
            {
                appAuthenticationTicket.RemoteIpAddress = remoteIpAddress;
            }

            var userAgent = httpContext.Request.Headers.UserAgent;

            if (!string.IsNullOrEmpty(userAgent))
            {
                var uaParser = UAParser.Parser.GetDefault();
                var clientInfo = uaParser.Parse(userAgent);
                appAuthenticationTicket.OperatingSystem = clientInfo.OS.ToString();
                appAuthenticationTicket.UserAgentFamily = clientInfo.UA.Family;
                appAuthenticationTicket.UserAgentVersion = $"{clientInfo.UA.Major}.{clientInfo.UA.Minor}.{clientInfo.UA.Patch}";
            }
        }

        var jsonAppAuthenticationTicket = JsonSerializer.Serialize(appAuthenticationTicket);
        await cache.StringSetAsync(appAuthenticationTicket.Key, jsonAppAuthenticationTicket, TimeSpan.FromDays(30));

        await cache.SetAddAsync($"UserTickets:{userId}", appAuthenticationTicket.Key);

        return appAuthenticationTicket.Key;
    }
}
