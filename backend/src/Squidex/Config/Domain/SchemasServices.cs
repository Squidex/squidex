// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.AI;
using Squidex.Domain.Apps.Entities.History;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Domain.Apps.Entities.Search;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Config.Domain;

public static class SchemasServices
{
    public static void AddSquidexSchemas(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<SchemasOptions>(config,
            "schemas");

        services.AddSingletonAs<SchemaPermanentDeleter>()
            .As<IEventConsumer>();

        services.AddSingletonAs<SchemasSearchSource>()
            .As<ISearchSource>();

        services.AddSingletonAs<SchemasChatTool>()
            .As<IChatToolProvider>();

        services.AddSingletonAs<SchemaHistoryEventsCreator>()
            .As<IHistoryEventsCreator>();
    }
}
