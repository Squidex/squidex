// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;

namespace Squidex.Domain.Apps.Entities;

public static class AppProviderExtensions
{
    public static async Task<ResolvedComponents> GetComponentsAsync(this IAppProvider appProvider, ISchemaEntity schema,
        CancellationToken ct = default)
    {
        Dictionary<DomainId, Schema>? result = null;

        var appId = schema.AppId.Id;

        async Task ResolveWithIdsAsync(IField field, ReadonlyList<DomainId>? schemaIds)
        {
            if (schemaIds == null)
            {
                return;
            }

            foreach (var schemaId in schemaIds)
            {
                if (schemaId == schema.Id)
                {
                    result ??= new Dictionary<DomainId, Schema>();
                    result[schemaId] = schema.SchemaDef;
                }
                else if (result == null || !result.TryGetValue(schemaId, out _))
                {
                    var resolvedEntity = await appProvider.GetSchemaAsync(appId, schemaId, false, ct);

                    if (resolvedEntity != null)
                    {
                        result ??= new Dictionary<DomainId, Schema>();
                        result[schemaId] = resolvedEntity.SchemaDef;

                        await ResolveSchemaAsync(resolvedEntity);
                    }
                }
            }
        }

        async Task ResolveArrayAsync(IArrayField arrayField)
        {
            foreach (var nestedField in arrayField.Fields)
            {
                await ResolveFieldAsync(nestedField);
            }
        }

        async Task ResolveFieldAsync(IField field)
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

        async Task ResolveSchemaAsync(ISchemaEntity schema)
        {
            foreach (var field in schema.SchemaDef.Fields)
            {
                await ResolveFieldAsync(field);
            }
        }

        await ResolveSchemaAsync(schema);

        if (result == null)
        {
            return ResolvedComponents.Empty;
        }

        return new ResolvedComponents(result);
    }
}
