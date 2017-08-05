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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Domain.Users;
using Squidex.Infrastructure;
using Squidex.Shared.Identity;
using Squidex.Shared.Users;
using StackExchange.Redis;

namespace Squidex.Config.Identity
{
    public static class IdentityServices
    {
        public static IServiceCollection AddMyDataProtectection(this IServiceCollection services, IConfiguration configuration)
        {
            var dataProtection = services.AddDataProtection().SetApplicationName("Squidex");

            var keyStoreType = configuration.GetValue<string>("identity:keysStore:type");

            if (string.IsNullOrWhiteSpace(keyStoreType))
            {
                throw new ConfigurationException("Configure KeyStore type with 'identity:keysStore:type'.");
            }

            if (string.Equals(keyStoreType, "Redis", StringComparison.OrdinalIgnoreCase))
            {
                var redisConfiguration = configuration.GetValue<string>("identity:keysStore:redis:configuration");

                if (string.IsNullOrWhiteSpace(redisConfiguration))
                {
                    throw new ConfigurationException("Configure KeyStore Redis configuration with 'identity:keysStore:redis:configuration'.");
                }

                var connectionMultiplexer = Singletons<ConnectionMultiplexer>.GetOrAdd(redisConfiguration, s => ConnectionMultiplexer.Connect(s));

                dataProtection.PersistKeysToRedis(connectionMultiplexer);
            }
            else if (string.Equals(keyStoreType, "Folder", StringComparison.OrdinalIgnoreCase))
            {
                var folderPath = configuration.GetValue<string>("identity:keysStore:folder:path");

                if (string.IsNullOrWhiteSpace(folderPath))
                {
                    throw new ConfigurationException("Configure KeyStore Folder path with 'identity:keysStore:folder:path'.");
                }

                dataProtection.PersistKeysToFileSystem(new DirectoryInfo(folderPath));
            }
            else if (!string.Equals(keyStoreType, "InMemory", StringComparison.OrdinalIgnoreCase))
            {
                throw new ConfigurationException($"Unsupported value '{keyStoreType}' for 'identity:keysStore:type', supported: Redis, Folder, InMemory.");
            }

            return services;
        }

        public static IServiceCollection AddMyIdentityServer(this IServiceCollection services)
        {
            X509Certificate2 certificate;

            var assembly = typeof(IdentityServices).GetTypeInfo().Assembly;

            using (var certStream = assembly.GetManifestResourceStream("Squidex.Config.Identity.Cert.IdentityCert.pfx"))
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
                    options.UserInteraction.ErrorUrl = "/error/";
                })
                .AddAspNetIdentity<IUser>()
                .AddInMemoryApiResources(GetApiResources())
                .AddInMemoryIdentityResources(GetIdentityResources())
                .AddSigningCredential(certificate);

            return services;
        }

        public static IServiceCollection AddMyIdentity(this IServiceCollection services)
        {
            services.AddIdentity<IUser, IRole>().AddDefaultTokenProviders();

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
                    SquidexClaimTypes.SquidexDisplayName,
                    SquidexClaimTypes.SquidexPictureUrl
                });
        }
    }
}
