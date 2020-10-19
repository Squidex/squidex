// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using IdentityModel;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Squidex.Domain.Users;
using Squidex.Shared.Identity;
using Squidex.Web;
using Squidex.Web.Pipeline;

namespace Squidex.Areas.IdentityServer.Config
{
    public static class IdentityServerServices
    {
        public static void AddSquidexIdentityServer(this IServiceCollection services)
        {
            services.AddSingletonAs<IConfigureOptions<KeyManagementOptions>>(s =>
            {
                return new ConfigureOptions<KeyManagementOptions>(options =>
                {
                    options.XmlRepository = s.GetRequiredService<IXmlRepository>();
                });
            });

            services.AddDataProtection().SetApplicationName("Squidex");

            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddDefaultTokenProviders();

            services.AddSingletonAs<DefaultXmlRepository>()
                .As<IXmlRepository>();

            services.AddSingletonAs<DefaultKeyStore>()
                .As<ISigningCredentialStore>().As<IValidationKeysStore>();

            services.AddSingletonAs<PwnedPasswordValidator>()
                .As<IPasswordValidator<IdentityUser>>();

            services.AddScopedAs<UserClaimsPrincipalFactoryWithEmail>()
                .As<IUserClaimsPrincipalFactory<IdentityUser>>();

            services.AddSingletonAs<ApiPermissionUnifier>()
                .As<IClaimsTransformation>();

            services.AddSingletonAs<LazyClientStore>()
                .As<IClientStore>();

            services.AddSingletonAs<InMemoryResourcesStore>()
                .As<IResourceStore>();

            services.AddIdentityServer(options =>
                {
                    options.UserInteraction.ErrorUrl = "/error/";
                })
                .AddAspNetIdentity<IdentityUser>()
                .AddInMemoryApiScopes(GetApiScopes())
                .AddInMemoryIdentityResources(GetIdentityResources());
        }

        private static IEnumerable<ApiScope> GetApiScopes()
        {
            yield return new ApiScope(Constants.ApiScope)
            {
                UserClaims = new List<string>
                {
                    JwtClaimTypes.Email,
                    JwtClaimTypes.Role,
                    SquidexClaimTypes.Permissions
                }
            };
        }

        private static IEnumerable<IdentityResource> GetIdentityResources()
        {
            yield return new IdentityResources.OpenId();
            yield return new IdentityResources.Profile();
            yield return new IdentityResources.Email();
            yield return new IdentityResource(Constants.RoleScope,
                new[]
                {
                    JwtClaimTypes.Role
                });
            yield return new IdentityResource(Constants.PermissionsScope,
                new[]
                {
                    SquidexClaimTypes.Permissions
                });
            yield return new IdentityResource(Constants.ProfileScope,
                new[]
                {
                    SquidexClaimTypes.DisplayName,
                    SquidexClaimTypes.PictureUrl,
                    SquidexClaimTypes.NotifoKey
                });
        }
    }
}
