using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuickTicketStoreDemo.Data;
using QuickTicketStoreDemo.Stores;

namespace QuickTicketStoreDemo.Extensions;

public static class IdentityExtensions
{
    public static IServiceCollection AddIdentityDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppIdentityDbContext>(o =>
        {
            o.UseSqlServer(configuration.GetConnectionString("IdentityConnection"));
        });
        return services;
    }

    public static IServiceCollection AddIdentityItself(this IServiceCollection services)
    {
        services.ConfigureApplicationCookie(c =>
        {
            c.LoginPath = new PathString("/Identity/Account/Login");
            c.ExpireTimeSpan = TimeSpan.FromMinutes(30);
            c.SlidingExpiration = true;
            c.Events = new CookieAuthenticationEvents
            {
                OnValidatePrincipal = SecurityStampValidator.ValidatePrincipalAsync
            };
            c.SessionStore = new AppTicketStore(services);
        });

        services.ConfigureExternalCookie(c =>
        {
            c.Cookie.Name = IdentityConstants.ExternalScheme;
            c.ExpireTimeSpan = TimeSpan.FromMinutes(30);
            c.SessionStore = new AppTicketStore(services);
        });

        services.AddIdentity<AppIdentityUser, AppIdentityRole>(o =>
        {
            o.Password.RequiredLength = 8;

            o.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);

            o.Lockout.MaxFailedAccessAttempts = 3;
            o.Lockout.AllowedForNewUsers = true;

            o.SignIn.RequireConfirmedAccount = false; // temp: no email serivce

            o.User.RequireUniqueEmail = true;
        })
            .AddEntityFrameworkStores<AppIdentityDbContext>()
            .AddDefaultUI()
            .AddDefaultTokenProviders();

        return services;
    }

    public static async Task MigrateIdentityDatabase(this WebApplication app)
    {
        var scope = app.Services.CreateScope();
        var serviceProvider = scope.ServiceProvider;
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

        try
        {
            using var appIdentityDbContext = serviceProvider.GetRequiredService<AppIdentityDbContext>();
            await appIdentityDbContext.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
        }
    }
}
