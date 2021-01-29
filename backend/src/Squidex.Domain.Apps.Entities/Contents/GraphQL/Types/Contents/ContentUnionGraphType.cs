// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using GraphQL.Types;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Contents
{
    internal sealed class ContentUnionGraphType : UnionGraphType
    {
        private readonly Dictionary<DomainId, IObjectGraphType> types = new Dictionary<DomainId, IObjectGraphType>();

        public ContentUnionGraphType(Builder builder, FieldInfo fieldInfo, ReferencesFieldProperties properties)
        {
            Name = fieldInfo.UnionType;

            if (properties.SchemaIds?.Any() == true)
            {
                foreach (var schemaId in properties.SchemaIds)
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
