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
using Squidex.Shared.Identity;
using Squidex.Shared.Users;

namespace Squidex.Areas.IdentityServer.Config
{
    public static class IdentityServerExtensions
    {
        public static IApplicationBuilder UseMyIdentityServer(this IApplicationBuilder app)
        {
             app.UseIdentityServer();

            return app;
        }

        public static IServiceProvider UseMyAdminRole(this IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IRole>>();
            var roleFactory = services.GetRequiredService<IRoleFactory>();

            roleManager.CreateAsync(roleFactory.Create(SquidexRoles.Administrator)).Wait();

            return services;
        }

        public static IServiceProvider UseMyAdmin(this IServiceProvider services)
        {
            var options = services.GetService<IOptions<MyIdentityOptions>>().Value;

            var userManager = services.GetService<UserManager<IUser>>();
            var userFactory = services.GetService<IUserFactory>();

            var log = services.GetService<ISemanticLog>();

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
                            var user = await userManager.CreateAsync(userFactory, adminEmail, adminEmail, adminPass);

                            await userManager.AddToRoleAsync(user, SquidexRoles.Administrator);
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
