// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Hosting;

namespace Squidex.Config.Orleans
{
    public static class SiloServices
    {
        public static void AddAppSiloServices(this IServiceCollection services, IConfiguration config)
        {
            config.ConfigureByOption("store:type", new Options
            {
                ["MongoDB"] = () =>
                {
                    var mongoConfiguration = config.GetRequiredValue("store:mongoDb:configuration");
                    var mongoDatabaseName = config.GetRequiredValue("store:mongoDb:database");

                    services.AddMongoDBMembershipTable(options =>
                    {
                        options.ConnectionString = mongoConfiguration;
                        options.CollectionPrefix = "Orleans_";
                        options.DatabaseName = mongoDatabaseName;
                    });

                    services.AddMongoDBReminders(options =>
                    {
                        options.ConnectionString = mongoConfiguration;
                        options.CollectionPrefix = "Orleans_";
                        options.DatabaseName = mongoDatabaseName;
                    });
                }
            });
        }
    }
}