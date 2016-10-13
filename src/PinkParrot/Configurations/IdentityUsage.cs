// ==========================================================================
//  IdentityUsage.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.MongoDB;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace PinkParrot.Configurations
{
    public static class IdentityUsage
    {
        public static void UseDefaultUser(this IApplicationBuilder app)
        {
            var options = app.ApplicationServices.GetService<IOptions<IdentityOptions>>().Value;

            var username = options.DefaultUsername;
            var userManager = app.ApplicationServices.GetService<UserManager<IdentityUser>>();

            if (!string.IsNullOrWhiteSpace(options.DefaultUsername) &&
                !string.IsNullOrWhiteSpace(options.DefaultPassword))
            {
                Task.Run(async () =>
                {
                    if (userManager.SupportsQueryableUsers && !userManager.Users.Any())
                    {
                        var user = new IdentityUser { UserName = username, Email = username, EmailConfirmed = true };

                        await userManager.CreateAsync(user, options.DefaultPassword);
                    }
                }).Wait();
            }
        }
    }
}
