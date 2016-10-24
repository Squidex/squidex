// ==========================================================================
//  IdentityDependencies.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using IdentityServer4.Stores.InMemory;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.MongoDB;
using Microsoft.Extensions.DependencyInjection;

namespace Squidex.Configurations.Identity
{
    public static class IdentityServices
    {
        public static IServiceCollection AddMyIdentityServer(this IServiceCollection services, IHostingEnvironment env)
        {
            var certPath = Path.Combine(env.ContentRootPath, "Configurations", "Identity", "Cert", "IdentityCert.pfx");

            var certificate = new X509Certificate2(certPath, "password");

            services.AddSingleton(
                GetScopes());
            services.AddSingleton<IClientStore,
                LazyClientStore>();
            services.AddSingleton<IScopeStore,
                InMemoryScopeStore>();

            services.AddIdentityServer().SetSigningCredential(certificate)
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
                    Name = Constants.ApiScope, Type = ScopeType.Resource
                }
            };
        }
    }
}
