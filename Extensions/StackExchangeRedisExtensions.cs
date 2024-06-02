using StackExchange.Redis;

namespace QuickTicketStoreDemo.Extensions;

public static class StackExchangeRedisExtensions
{
    public static IServiceCollection AddAppStackExchangeRedisCache(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IConnectionMultiplexer>(s =>
        {
            var connectionString = configuration.GetConnectionString("Redis")
            ?? throw new InvalidOperationException(nameof(IConfiguration));

            return ConnectionMultiplexer.Connect(ConfigurationOptions.Parse(connectionString));
        });
        return services;
    }
}
