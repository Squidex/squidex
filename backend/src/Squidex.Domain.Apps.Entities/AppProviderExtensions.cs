// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;

namespace Squidex.Domain.Apps.Entities
{
    public static class AppProviderExtensions
    {
        public static async Task<ResolvedComponents> GetComponentsAsync(this IAppProvider appProvider, ISchemaEntity schema)
        {
            Dictionary<DomainId, Schema>? result = null;

            var appId = schema.AppId.Id;

            async Task ResolveWithIdsAsync(IField field, ImmutableList<DomainId>? schemaIds)
            {
                if (schemaIds != null)
                {
                    foreach (var schemaId in schemaIds)
                    {
                        if (result == null || !result.TryGetValue(schemaId, out _))
                        {
                            var resolvedEntity = await appProvider.GetSchemaAsync(appId, schemaId, true);

                            if (resolvedEntity != null)
                            {
                                result ??= new Dictionary<DomainId, Schema>();
                                result[schemaId] = resolvedEntity.SchemaDef;
                            }
                        }
                    }
                }
            }

            async Task ResolveArrayAsync(IArrayField arrayField)
            {
                foreach (var nestedField in arrayField.Fields)
                {
                    await ResolveAsync(nestedField);
                }
            }

            async Task ResolveAsync(IField field)
            {
                switch (field)
                {
                    case IField<ComponentFieldProperties> component:
                        await ResolveWithIdsAsync(field, component.Properties.SchemaIds);
                        break;

                    case IField<ComponentsFieldProperties> components:
                        await ResolveWithIdsAsync(field, components.Properties.SchemaIds);
                        break;

                    case IArrayField arrayField:
                        await ResolveArrayAsync(arrayField);
                        break;
                }
            }

            foreach (var field in schema.SchemaDef.Fields)
            {
                await ResolveAsync(field);
            }

            if (result == null)
            {
                return ResolvedComponents.Empty;
            }

            return new ResolvedComponents(result);
        }
    }
}
