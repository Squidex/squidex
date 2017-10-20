﻿// ==========================================================================
//  IdentityUsage.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Squidex.Domain.Users;
using Squidex.Infrastructure.Log;
using Squidex.Shared.Identity;
using Squidex.Shared.Users;

namespace Squidex.Config.Identity
{
    public static class IdentityUsage
    {
        public static IApplicationBuilder UseMyIdentityServer(this IApplicationBuilder app)
        {
             app.UseIdentityServer();

            return app;
        }

        public static IApplicationBuilder UseMyAdminRole(this IApplicationBuilder app)
        {
            var roleManager = app.ApplicationServices.GetRequiredService<RoleManager<IRole>>();
            var roleFactory = app.ApplicationServices.GetRequiredService<IRoleFactory>();

            roleManager.CreateAsync(roleFactory.Create(SquidexRoles.Administrator)).Wait();

            return app;
        }

        public static IApplicationBuilder UseMyAdmin(this IApplicationBuilder app)
        {
            var options = app.ApplicationServices.GetService<IOptions<MyIdentityOptions>>().Value;

            var userManager = app.ApplicationServices.GetService<UserManager<IUser>>();
            var userFactory = app.ApplicationServices.GetService<IUserFactory>();

            var log = app.ApplicationServices.GetService<ISemanticLog>();

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

            return app;
        }
    }
}
