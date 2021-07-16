// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using GraphQL.Types;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Contents
{
    internal sealed class ReferenceUnionGraphType : UnionGraphType
    {
        private readonly Dictionary<DomainId, IObjectGraphType> types = new Dictionary<DomainId, IObjectGraphType>();

        public bool HasType => types.Count > 0;

        public ReferenceUnionGraphType(Builder builder, FieldInfo fieldInfo, ImmutableList<DomainId>? schemaIds)
        {
            // The name is used for equal comparison. Therefore it is important to treat it as readonly.
            Name = fieldInfo.ReferenceType;

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

            if (HasType)
            {
                foreach (var type in types)
                {
                    AddPossibleType(type.Value);
                }

                ResolveType = value =>
                {
                    if (value is IContentEntity content)
                    {
                        return types.GetOrDefault(content.SchemaId.Id);
                    }

                    return null;
                };
            }
        }
    }
}
