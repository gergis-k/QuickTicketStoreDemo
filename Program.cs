using QuickTicketStoreDemo.Extensions;

namespace QuickTicketStoreDemo;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddIdentityDatabase(builder.Configuration);
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        builder.Services.AddIdentityItself();

        builder.Services.AddAppStackExchangeRedisCache(builder.Configuration);

        builder.Services.AddControllersWithViews();

        builder.Services.Configure<CookieOptions>(cnfg =>
        {
            cnfg.HttpOnly = true;
            cnfg.Secure = true;
            cnfg.SameSite = SameSiteMode.Strict;
        });

        var app = builder.Build();

        await app.MigrateIdentityDatabase();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseMigrationsEndPoint();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");
        app.MapRazorPages();

        app.Run();
    }
}
