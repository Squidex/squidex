// ==========================================================================
//  IdentityServices.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using IdentityModel;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.MongoDB;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Security;
using StackExchange.Redis;

namespace Squidex.Config.Identity
{
    public static class IdentityServices
    {
        public static IServiceCollection AddMyDataProtectection(this IServiceCollection services, IConfiguration configuration)
        {
            var dataProtection = services.AddDataProtection().SetApplicationName("Squidex");

            var clustererType = configuration.GetValue<string>("squidex:clusterer:type");

            if (clustererType.Equals("redis", StringComparison.OrdinalIgnoreCase))
            {
                var connectionString = configuration.GetValue<string>("squidex:clusterer:redis:connectionString");

                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    throw new ConfigurationException("You must specify the Redis connection string in the 'squidex:clusterer:redis:connectionString' configuration section.");
                }
                
                var connectionMultiplexer = ConnectionMultiplexer.Connect(connectionString);

                dataProtection.PersistKeysToRedis(connectionMultiplexer);
            }
            else
            {
                var keysFolder = configuration.GetValue<string>("squidex:identity:keysFolder");

                if (!string.IsNullOrWhiteSpace(keysFolder))
                {
                    dataProtection.PersistKeysToFileSystem(new DirectoryInfo(keysFolder));
                }
            }

            return services;
        }

        public static IServiceCollection AddMyIdentityServer(this IServiceCollection services, IHostingEnvironment env)
        {
            X509Certificate2 certificate;

            var assemblyName = new AssemblyName("Squidex");
            var assemblyRef = Assembly.Load(assemblyName);

            using (var certStream = assemblyRef.GetManifestResourceStream("Squidex.Config.Identity.Cert.IdentityCert.pfx"))
            {
                var certData = new byte[certStream.Length];

                certStream.Read(certData, 0, certData.Length);
                certificate = new X509Certificate2(certData, "password", 
                    X509KeyStorageFlags.MachineKeySet |
                    X509KeyStorageFlags.PersistKeySet |
                    X509KeyStorageFlags.Exportable);
            }

            services.AddSingleton(
                GetApiResources());
            services.AddSingleton(
                GetIdentityResources());
            services.AddSingleton<IClientStore,
                LazyClientStore>();
            services.AddSingleton<IResourceStore,
                InMemoryResourcesStore>();

            services.AddIdentityServer(options =>
                {
                    options.UserInteraction.ErrorUrl = "/account/error/";
                })
                .AddAspNetIdentity<IdentityUser>()
                .AddInMemoryApiResources(GetApiResources())
                .AddInMemoryIdentityResources(GetIdentityResources())
                .AddSigningCredential(certificate);

            return services;
        }

        public static IServiceCollection AddMyIdentity(this IServiceCollection services)
        {
            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddDefaultTokenProviders();

            return services;
        }

        private static IEnumerable<ApiResource> GetApiResources()
        {
            yield return new ApiResource(Constants.ApiScope)
            {
                UserClaims = new List<string>
                {
                    JwtClaimTypes.Role
                }
            };
        }

        private static IEnumerable<IdentityResource> GetIdentityResources()
        {
            yield return new IdentityResources.OpenId();
            yield return new IdentityResources.Profile();
            yield return new IdentityResources.Profile();
            yield return new IdentityResource(Constants.RoleScope,
                new[]
                {
                    JwtClaimTypes.Role
                });
            yield return new IdentityResource(Constants.ProfileScope,
                new[]
                {
                    ExtendedClaimTypes.SquidexDisplayName,
                    ExtendedClaimTypes.SquidexPictureUrl
                });
        }
    }
}
