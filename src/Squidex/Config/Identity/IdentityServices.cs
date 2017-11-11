// ==========================================================================
//  IdentityServices.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.IO;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Infrastructure;
using StackExchange.Redis;

namespace Squidex.Config.Identity
{
    public static class IdentityServices
    {
        public static IServiceCollection AddMyDataProtectection(this IServiceCollection services, IConfiguration configuration)
        {
            var dataProtection = services.AddDataProtection().SetApplicationName("Squidex");

            configuration.ConfigureByOption("identity:keysStore:type", new Options
            {
                ["Redis"] = () =>
                {
                    var redisConfiguration = configuration.GetRequiredValue("identity:keysStore:redis:configuration");

                    var connectionMultiplexer = Singletons<ConnectionMultiplexer>.GetOrAdd(redisConfiguration, s => ConnectionMultiplexer.Connect(s));

                    dataProtection.PersistKeysToRedis(connectionMultiplexer);
                },
                ["Folder"] = () =>
                {
                    var folderPath = configuration.GetRequiredValue("identity:keysStore:folder:path");

                    dataProtection.PersistKeysToFileSystem(new DirectoryInfo(folderPath));
                },
                ["InMemory"] = () => { }
            });

            return services;
        }
    }
}
