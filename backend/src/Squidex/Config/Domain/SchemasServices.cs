// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.DependencyInjection;
using Squidex.Domain.Apps.Entities.History;
using Squidex.Domain.Apps.Entities.Schemas;

namespace Squidex.Config.Domain
{
    public static class SchemasServices
    {
        public static void AddSquidexSchemas(this IServiceCollection services)
        {
            services.AddTransientAs<SchemaDomainObject>()
                .AsSelf();

            services.AddSingletonAs<SchemaHistoryEventsCreator>()
                .As<IHistoryEventsCreator>();
        }
    }
}