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

namespace Squidex.Config.Domain;

public static class SchemasServices
{
    public static void AddSquidexSchemas(this IServiceCollection services)
    {
        services.AddSingletonAs<SchemasSearchSource>()
            .As<ISearchSource>();

        services.AddSingletonAs<SchemasChatTool>()
            .As<IChatToolProvider>();

        services.AddSingletonAs<SchemaHistoryEventsCreator>()
            .As<IHistoryEventsCreator>();
    }
}
