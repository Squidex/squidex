// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.History;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Domain.Apps.Entities.Search;
using Squidex.Infrastructure.Commands;

namespace Squidex.Config.Domain;

public static class SchemasServices
{
    public static void AddSquidexSchemas(this IServiceCollection services)
    {
        services.AddSingletonAs<MigrateFieldNamesCommandMiddleware>()
            .As<ICommandMiddleware>();

        services.AddTransientAs<SchemasSearchSource>()
            .As<ISearchSource>();

        services.AddSingletonAs<SchemaHistoryEventsCreator>()
            .As<IHistoryEventsCreator>();
    }
}
