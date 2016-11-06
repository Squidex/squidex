// ==========================================================================
//  IdentityServices.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using IdentityServer4.Stores.InMemory;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.MongoDB;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Infrastructure.Security;

namespace Squidex.Configurations.Identity
{
    public static class IdentityServices
    {
        public static IServiceCollection AddMyIdentityServer(this IServiceCollection services, IHostingEnvironment env)
        {
            X509Certificate2 certificate;

            var assemblyName = new AssemblyName("Squidex");
            var assemblyRef = Assembly.Load(assemblyName);

            using (var certStream = assemblyRef.GetManifestResourceStream("Squidex.Configurations.Identity.Cert.IdentityCert.pfx"))
            {
                var certData = new byte[certStream.Length];

                certStream.Read(certData, 0, certData.Length);
                certificate = new X509Certificate2(certData, "password", 
                    X509KeyStorageFlags.MachineKeySet |
                    X509KeyStorageFlags.PersistKeySet |
                    X509KeyStorageFlags.Exportable);
            }

            services.AddSingleton(
                GetScopes());
            services.AddSingleton<IClientStore,
                LazyClientStore>();
            services.AddSingleton<IScopeStore,
                InMemoryScopeStore>();

            services.AddIdentityServer(options =>
                {
                    options.UserInteractionOptions.ErrorUrl = "/account/error/";
                })
                .SetSigningCredential(certificate)
                .AddAspNetIdentity<IdentityUser>();

            return services;
        }

        public static IServiceCollection AddMyIdentity(this IServiceCollection services)
        {
            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddDefaultTokenProviders();

            return services;
        }

        public static IEnumerable<Scope> GetScopes()
        {
            return new List<Scope>
            {
                StandardScopes.OpenId,
                StandardScopes.Profile,
                new Scope
                {
                    Name = Constants.ProfileScope, Type = ScopeType.Identity,
                    Claims = new List<ScopeClaim>
                    {
                        new ScopeClaim(ExtendedClaimTypes.SquidexDisplayName, true),
                        new ScopeClaim(ExtendedClaimTypes.SquidexPictureUrl, true)
                    }
                },
                new Scope
                {
                    Name = Constants.ApiScope, Type = ScopeType.Resource
                }
            };
        }
    }
}
