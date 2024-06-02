using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace QuickTicketStoreDemo.Data;

public class AppIdentityDbContext : IdentityDbContext<AppIdentityUser, AppIdentityRole, string>
{
    protected AppIdentityDbContext() { }

    public AppIdentityDbContext(DbContextOptions<AppIdentityDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<AppIdentityRole>(b =>
        {
            b.Property(p => p.TagText).HasMaxLength(16);
        });

        builder.Entity<AppIdentityUser>(b =>
        {
            b.Property(p => p.AvatarUrl).HasMaxLength(1024);
            b.Property(p => p.Gender).HasMaxLength(8);
            b.Property(p => p.DisplayName).HasMaxLength(64);
        });
    }
}
