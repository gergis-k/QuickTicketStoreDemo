// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using QuickTicketStoreDemo.Data;
using QuickTicketStoreDemo.Data.Operations;
using StackExchange.Redis;
using System.Text.Json;

namespace QuickTicketStoreDemo.Areas.Identity.Pages.Account.Manage
{
    public class LoggedInDevicesModel : PageModel
    {
        private readonly UserManager<AppIdentityUser> _userManager;
        private readonly IDatabase _cache;
        private readonly IDataProtector _dataProtector;
        private readonly ILogger<PersonalDataModel> _logger;

        public IList<AppAuthenticationTicket?> Tickets { get; set; } = [];

        public LoggedInDevicesModel(
            IDataProtectionProvider dataProtectionProvider,
            IConnectionMultiplexer connectionMultiplexer,
            UserManager<AppIdentityUser> userManager,
            ILogger<PersonalDataModel> logger)
        {
            _userManager = userManager;
            _cache = connectionMultiplexer.GetDatabase();
            _logger = logger;
            _dataProtector = dataProtectionProvider.CreateProtector(nameof(LoggedInDevicesModel));
        }

        public async Task<IActionResult> OnGet()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadData(user);

            return Page();
        }

        private async Task LoadData(AppIdentityUser user)
        {
            var userId = await _userManager.GetUserIdAsync(user);

            var keys = await _cache.SetMembersAsync($"UserTickets:{userId}");

            foreach (var key in keys)
            {
                var ticketFromCache = await _cache.StringGetAsync(key.ToString());
                var deserializedTicket = JsonSerializer.Deserialize<AppAuthenticationTicket?>(ticketFromCache.ToString());
                Tickets.Add(deserializedTicket);
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(string? id = null)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!string.IsNullOrEmpty(id) && Guid.TryParse(id, out _))
            {
                var ticketFromCache = await _cache.StringGetAsync(id);
                if (ticketFromCache.HasValue)
                {
                    var deserializedTicket = JsonSerializer.Deserialize<AppAuthenticationTicket?>(ticketFromCache.ToString());

                    if (deserializedTicket is not null)
                    {
                        var userId = deserializedTicket.UserId;
                        await _cache.SetRemoveAsync($"UserTickets:{userId}", deserializedTicket.Key);
                        await _cache.KeyDeleteAsync(deserializedTicket.Key);
                    }
                }
            }

            return RedirectToPage("./LoggedInDevices");
        }
    }
}
