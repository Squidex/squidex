// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using GraphQL.Types;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types
{
    public sealed class ContentUnionGraphType : UnionGraphType
    {
        private readonly Dictionary<Guid, IObjectGraphType> types = new Dictionary<Guid, IObjectGraphType>();

        public ContentUnionGraphType(string fieldName, Dictionary<Guid, ContentGraphType> schemaTypes, IEnumerable<Guid>? schemaIds)
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
                foreach (var schemaType in schemaTypes)
                {
                    types[schemaType.Key] = schemaType.Value;
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
