using Microsoft.AspNetCore.Identity;

namespace QuickTicketStoreDemo.Data;

public class AppIdentityRole : IdentityRole
{
    public string TagText { get; set; } = default!;
}
