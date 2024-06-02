namespace QuickTicketStoreDemo.Data.Operations;

public class AppAuthenticationTicket
{
    public string Key { get; set; } = $"{Guid.NewGuid():N}";

    public string UserId { get; set; } = default!;

    public string Value { get; set; } = default!;

    public DateTimeOffset? LastActive { get; set; }

    public DateTimeOffset? Expires { get; set; }

    public string? RemoteIpAddress { get; set; }

    public string? OperatingSystem { get; set; }

    public string? UserAgentFamily { get; set; }

    public string? UserAgentVersion { get; set; }
}
