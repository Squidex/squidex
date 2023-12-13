// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL.Types;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Contents;

internal sealed class ContentUnionGraphType : UnionGraphType
{
    private readonly Dictionary<DomainId, IObjectGraphType> types = [];

    // We need the schema identity at runtime.
    public IReadOnlyDictionary<DomainId, IObjectGraphType> SchemaTypes => types;

    public ContentUnionGraphType(Builder builder, string name, ReadonlyList<DomainId>? schemaIds)
    {
        // The name is used for equal comparison. Therefore it is important to treat it as readonly.
        Name = name;

        if (schemaIds?.Any() == true)
        {
            foreach (var schemaId in schemaIds)
            {
                var contentType = builder.GetContentType(schemaId);

                if (contentType != null)
                {
                    types[schemaId] = contentType;
                }
            }
        }
        else
        {
            foreach (var (key, value) in builder.GetAllContentTypes())
            {
                types[key.Schema.Id] = value;
            }
        }

        if (SchemaTypes.Count > 0)
        {
            foreach (var type in types)
            {
                AddPossibleType(type.Value);
            }

            ResolveType = value =>
            {
                if (value is Content content)
                {
                    return types.GetValueOrDefault(content.SchemaId.Id);
                }

                return null;
            };
        }
    }
}
