// ==========================================================================
//  IdentityServerServices.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using IdentityModel;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Config;
using Squidex.Domain.Users;
using Squidex.Domain.Users.DataProtection.Orleans;
using Squidex.Shared.Identity;
using Squidex.Shared.Users;

namespace Squidex.Areas.IdentityServer.Config
{
    public static class IdentityServerServices
    {
        public static void AddMyIdentityServer(this IServiceCollection services)
        {
            X509Certificate2 certificate;

            var assembly = typeof(IdentityServerServices).GetTypeInfo().Assembly;

            using (var certStream = assembly.GetManifestResourceStream("Squidex.Areas.IdentityServer.Config.Cert.IdentityCert.pfx"))
            {
                var certData = new byte[certStream.Length];

                certStream.Read(certData, 0, certData.Length);
                certificate = new X509Certificate2(certData, "password",
                    X509KeyStorageFlags.MachineKeySet |
                    X509KeyStorageFlags.PersistKeySet |
                    X509KeyStorageFlags.Exportable);
            }

            services.AddIdentity<IUser, IRole>()
                .AddDefaultTokenProviders();

            services.AddDataProtection().SetApplicationName("Squidex");

            services.AddSingleton(GetApiResources());
            services.AddSingleton(GetIdentityResources());

            services.AddSingleton<IXmlRepository,
                OrleansXmlRepository>();
            services.AddSingleton<IUserClaimsPrincipalFactory<IUser>,
                UserClaimsPrincipalFactoryWithEmail>();
            services.AddSingleton<IClientStore,
                LazyClientStore>();
            services.AddSingleton<IResourceStore,
                InMemoryResourcesStore>();

            services.AddIdentityServer(options =>
                {
                    options.UserInteraction.ErrorUrl = "/error/";
                })
                .AddAspNetIdentity<IUser>()
                .AddInMemoryApiResources(GetApiResources())
                .AddInMemoryIdentityResources(GetIdentityResources())
                .AddSigningCredential(certificate);
        }

        private static IEnumerable<ApiResource> GetApiResources()
        {
            yield return new ApiResource(Constants.ApiScope)
            {
                UserClaims = new List<string>
                {
                    JwtClaimTypes.Email,
                    JwtClaimTypes.Role
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
            yield return new IdentityResource(Constants.ProfileScope,
                new[]
                {
                    SquidexClaimTypes.SquidexDisplayName,
                    SquidexClaimTypes.SquidexPictureUrl
                });
        }
    }
}
