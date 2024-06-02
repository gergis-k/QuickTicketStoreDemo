using Microsoft.AspNetCore.Identity;

namespace QuickTicketStoreDemo.Data;

public class AppIdentityUser : IdentityUser
{
    public string? DisplayName { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public string? Gender { get; set; }

    public string? AvatarUrl { get; set; }
}
