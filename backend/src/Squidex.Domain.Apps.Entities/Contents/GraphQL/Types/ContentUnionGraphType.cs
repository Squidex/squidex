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

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types
{
    public sealed class ContentUnionGraphType : UnionGraphType
    {
        private readonly Dictionary<DomainId, IObjectGraphType> types = new Dictionary<DomainId, IObjectGraphType>();

        public ContentUnionGraphType(string fieldName, Dictionary<DomainId, ContentGraphType> schemaTypes, IEnumerable<DomainId>? schemaIds)
        {
            Name = $"{fieldName}ReferenceUnionDto";

            if (schemaIds?.Any() == true)
            {
                foreach (var schemaId in schemaIds)
                {
                    var schemaType = schemaTypes.GetOrDefault(schemaId);

                    if (schemaType != null)
                    {
                        types[schemaId] = schemaType;
                    }
                }
            }
            else
            {
                foreach (var (key, value) in schemaTypes)
                {
                    types[key] = value;
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
