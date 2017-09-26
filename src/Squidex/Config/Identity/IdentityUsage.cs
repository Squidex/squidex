// ==========================================================================
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
using Squidex.Infrastructure;
using Squidex.Infrastructure.Log;
using Squidex.Shared.Identity;
using Squidex.Shared.Users;

namespace Squidex.Config.Identity
{
    public static class IdentityUsage
    {
        public static IApplicationBuilder UseMyIdentity(this IApplicationBuilder app)
        {
            app.UseIdentity();

            return app;
        }

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

        public static IApplicationBuilder UseMyApiProtection(this IApplicationBuilder app)
        {
            var apiScope = Constants.ApiScope;

            var urlsOptions = app.ApplicationServices.GetService<IOptions<MyUrlsOptions>>().Value;

            if (!string.IsNullOrWhiteSpace(urlsOptions.BaseUrl))
            {
                var identityOptions = app.ApplicationServices.GetService<IOptions<MyIdentityOptions>>().Value;

                string apiAuthorityUrl;

                if (!string.IsNullOrWhiteSpace(identityOptions.AuthorityUrl))
                {
                    apiAuthorityUrl = identityOptions.AuthorityUrl.BuildFullUrl(Constants.IdentityPrefix);
                }
                else
                {
                    apiAuthorityUrl = urlsOptions.BuildUrl(Constants.IdentityPrefix);
                }

                app.UseIdentityServerAuthentication(new IdentityServerAuthenticationOptions
                {
                    Authority = apiAuthorityUrl,
                    ApiName = apiScope,
                    ApiSecret = null,
                    RequireHttpsMetadata = identityOptions.RequiresHttps
                });
            }

            return app;
        }
    }
}
