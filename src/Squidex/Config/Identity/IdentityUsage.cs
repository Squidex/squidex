// ==========================================================================
//  IdentityUsage.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Squidex.Domain.Users;
using Squidex.Shared.Identity;
using Squidex.Shared.Users;

// ReSharper disable InvertIf

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

            if (options.IsAdminConfigured())
            {
                var adminEmail = options.AdminEmail;
                var adminPass = options.AdminPassword;

                Task.Run(async () =>
                {
                    var user = await userManager.FindByEmailAsync(adminEmail);

                    async Task userInitAsync(IUser theUser)
                    {
                        await userManager.RemovePasswordAsync(theUser);
                        await userManager.ChangePasswordAsync(theUser, null, adminPass);
                        await userManager.AddToRoleAsync(theUser, SquidexRoles.Administrator);
                    }

                    if (user != null)
                    {
                        if (options.EnforceAdmin)
                        {
                            await userInitAsync(user);
                        }
                    }
                    else if ((userManager.SupportsQueryableUsers && !userManager.Users.Any()) || options.EnforceAdmin)
                    {
                        user = userFactory.Create(adminEmail);

                        await userManager.CreateAsync(user);
                        await userInitAsync(user);
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
                var apiAuthorityUrl = urlsOptions.BuildUrl(Constants.IdentityPrefix);

                var identityOptions = app.ApplicationServices.GetService<IOptions<MyIdentityOptions>>().Value;

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
