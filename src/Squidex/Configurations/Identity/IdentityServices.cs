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
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.MongoDB;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Squidex.Configurations.Identity
{
    public static class IdentityServices
    {
        public static IServiceCollection AddMyIdentityServer(this IServiceCollection services, IHostingEnvironment env)
        {
            var certPath = Path.Combine(env.ContentRootPath, "Configurations", "Identity", "Cert", "IdentityCert.pfx");

            var certificate = new X509Certificate2(certPath, "password");

            services.AddIdentityServer()
                .SetSigningCredential(certificate)
                .AddInMemoryScopes(GetScopes())
                .AddInMemoryClients(GetClients())
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
                    Name = "api1",
                    Description = "My API"
                }
            };
        }

        public static IEnumerable<Client> GetClients()
        {
            return new List<Client>
            {
                new Client
                {
                    ClientId = "management-portal",
                    ClientName = "MVC Client",
                    RedirectUris = new List<string>
                    {
                        "http://localhost:5000/account/client-silent",
                        "http://localhost:5000/account/client-popup"
                    },
                    AllowedGrantTypes = GrantTypes.Implicit,
                    AllowedScopes = new List<string>
                    {
                        StandardScopes.OpenId.Name,
                        StandardScopes.Profile.Name
                    },
                    RequireConsent = false
                }
            };
        }
    }
}
