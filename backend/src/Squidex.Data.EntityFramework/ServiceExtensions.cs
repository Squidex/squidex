// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Squidex.AI;
using Squidex.Data.EntityFramework;
using YDotNet.Server.EntityFramework;

namespace Squidex;

public static class ServiceExtensions
{
    public static void AddSquidexEntityFramework(this IServiceCollection services, IConfiguration config)
    {
        services.AddYDotNet()
            .AddEntityFrameworkStorage<AppDbContext>();

        services.AddAI()
            .AddEntityFrameworkChatStore<AppDbContext>();

        services.AddMessaging()
            .AddEntityFrameworkDataStore<AppDbContext>(config);

        services.AddOpenIddict()
            .AddCore(builder =>
            {
                builder.UseEntityFrameworkCore()
                    .UseDbContext<AppDbContext>();
            });
    }
}
