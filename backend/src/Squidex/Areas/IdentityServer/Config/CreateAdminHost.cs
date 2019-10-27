// ==========================================================================
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
using Squidex.Config.Startup;
using Squidex.Domain.Users;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Security;
using Squidex.Shared;

namespace Squidex.Areas.IdentityServer.Config
{
    public sealed class CreateAdminHost : SafeHostedService
    {
        private readonly IServiceProvider serviceProvider;
        private readonly MyIdentityOptions identityOptions;

        public CreateAdminHost(ISemanticLog log, IServiceProvider serviceProvider, IOptions<MyIdentityOptions> identityOptions)
            : base(log)
        {
            this.serviceProvider = serviceProvider;

            this.identityOptions = identityOptions.Value;
        }

        protected override async Task StartAsync(ISemanticLog log, CancellationToken ct)
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
                }
            }
        }
    }
}
