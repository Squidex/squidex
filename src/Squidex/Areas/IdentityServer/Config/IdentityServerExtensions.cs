// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Squidex.Config;
using Squidex.Domain.Users;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Security;
using Squidex.Shared;

namespace Squidex.Areas.IdentityServer.Config
{
    public static class IdentityServerExtensions
    {
        public static IApplicationBuilder UseMyIdentityServer(this IApplicationBuilder app)
        {
             app.UseIdentityServer();

            return app;
        }

        public static IServiceProvider UseMyAdmin(this IServiceProvider services)
        {
            var options = services.GetRequiredService<IOptions<MyIdentityOptions>>().Value;

            var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
            var userFactory = services.GetRequiredService<IUserFactory>();

            var log = services.GetRequiredService<ISemanticLog>();

            if (options.IsAdminConfigured())
            {
                var adminEmail = options.AdminEmail;
                var adminPass = options.AdminPassword;

                Task.Run(async () =>
                {
                    if (userManager.SupportsQueryableUsers && !userManager.Users.Any())
                    {
                        try
                        {
                            var values = new UserValues
                            {
                                Email = adminEmail,
                                Password = adminPass,
                                Permissions = new PermissionSet(Permissions.Admin),
                                DisplayName = adminEmail
                            };

                            await userManager.CreateAsync(userFactory, values);
                        }
                        catch (Exception ex)
                        {
                            log.LogError(ex, w => w
                                .WriteProperty("action", "createAdmin")
                                .WriteProperty("status", "failed"));
                        }
                    }
                }).Wait();
            }

            return services;
        }
    }
}
