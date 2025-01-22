// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.States;

namespace Squidex.Config.Domain;

public static class StoreServices
{
    public static void AddSquidexStoreServices(this IServiceCollection services, IConfiguration config)
    {
        config.ConfigureByOption("store:type", new Alternatives
        {
            ["MongoDB"] = () =>
            {
                services.AddSquidexMongoStore(config);
            },
            ["Sql"] = () =>
            {
                services.AddSquidexEntityFramework(config);
            },
        });

        services.AddSingleton(typeof(IStore<>),
            typeof(Store<>));

        services.AddSingleton(typeof(IPersistenceFactory<>),
            typeof(Store<>));
    }
}
