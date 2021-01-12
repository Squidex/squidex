﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Logging;
using Squidex.Config;
using Squidex.Domain.Users;
using Squidex.Hosting;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Security;
using Squidex.Log;
using Squidex.Shared;
using Squidex.Shared.Users;

namespace Squidex.Areas.IdentityServer.Config
{
    public sealed class CreateAdminInitializer : IInitializable
    {
        private readonly IServiceProvider serviceProvider;
        private readonly MyIdentityOptions identityOptions;

        public int Order => int.MaxValue;

        public CreateAdminInitializer(IServiceProvider serviceProvider, IOptions<MyIdentityOptions> identityOptions)
        {
            this.serviceProvider = serviceProvider;

            this.identityOptions = identityOptions.Value;
        }

        public async Task InitializeAsync(CancellationToken ct)
        {
            IdentityModelEventSource.ShowPII = identityOptions.ShowPII;

            if (identityOptions.IsAdminConfigured())
            {
                using (var scope = serviceProvider.CreateScope())
                {
                    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
                    var userFactory = scope.ServiceProvider.GetRequiredService<IUserFactory>();

                    var adminEmail = identityOptions.AdminEmail;
                    var adminPass = identityOptions.AdminPassword;

                    var isEmpty = IsEmpty(userManager);

                    if (isEmpty || identityOptions.AdminRecreate)
                    {
                        try
                        {
                            var user = await userManager.FindByEmailWithClaimsAsync(adminEmail);

                            if (user != null)
                            {
                                if (identityOptions.AdminRecreate)
                                {
                                    var permissions = CreatePermissions(user.Permissions());

                                    var values = new UserValues
                                    {
                                        Password = adminPass,
                                        Permissions = permissions
                                    };

                                    await userManager.UpdateAsync(user.Identity, values);
                                }
                            }
                            else
                            {
                                var permissions = CreatePermissions(PermissionSet.Empty);

                                var values = new UserValues
                                {
                                    Email = adminEmail,
                                    Password = adminPass,
                                    Permissions = permissions,
                                    DisplayName = adminEmail
                                };

                                await userManager.CreateAsync(userFactory, values);
                            }
                        }
                        catch (Exception ex)
                        {
                            var log = serviceProvider.GetRequiredService<ISemanticLog>();

                            log.LogError(ex, w => w
                                .WriteProperty("action", "createAdmin")
                                .WriteProperty("status", "failed"));
                        }
                    }
                }
            }
        }

        private PermissionSet CreatePermissions(PermissionSet permissions)
        {
            permissions = permissions.Add(Permissions.Admin);

            foreach (var app in identityOptions.AdminApps.OrEmpty())
            {
                permissions = permissions.Add(Permissions.ForApp(Permissions.AppAdmin, app));
            }

            return permissions;
        }

        private static bool IsEmpty(UserManager<IdentityUser> userManager)
        {
            return userManager.SupportsQueryableUsers && !userManager.Users.Any();
        }
    }
}
