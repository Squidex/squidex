// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Logging;
using Squidex.Config;
using Squidex.Domain.Users;
using Squidex.Infrastructure.Security;
using Squidex.Infrastructure.Tasks;
using Squidex.Log;
using Squidex.Shared;

namespace Squidex.Areas.IdentityServer.Config
{
    public static class IdentityServerExtensions
    {
        public static IApplicationBuilder UseSquidexIdentityServer(this IApplicationBuilder app)
        {
             app.UseIdentityServer();

             return app;
        }

        public static IServiceProvider UseSquidexAdmin(this IServiceProvider services)
        {
            var options = services.GetRequiredService<IOptions<MyIdentityOptions>>().Value;

            IdentityModelEventSource.ShowPII = options.ShowPII;

            var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
            var userFactory = services.GetRequiredService<IUserFactory>();

            var log = services.GetRequiredService<ISemanticLog>();

            if (options.IsAdminConfigured())
            {
                var adminEmail = options.AdminEmail;
                var adminPass = options.AdminPassword;

                AsyncHelper.Sync(async () =>
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
                });
            }

            return services;
        }
    }
}
