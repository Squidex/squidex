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
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types
{
    public sealed class ReferenceGraphType : UnionGraphType
    {
        private readonly Dictionary<Guid, IObjectGraphType> types = new Dictionary<Guid, IObjectGraphType>();

        public ReferenceGraphType(string fieldName, IDictionary<ISchemaEntity, ContentGraphType> schemaTypes, IEnumerable<Guid> schemaIds, Func<Guid, IObjectGraphType> schemaResolver)
        {
            Name = $"{fieldName}ReferenceUnionDto";

            if (schemaIds?.Any() == true)
            {
                foreach (var schemaId in schemaIds)
                {
                    var schemaType = schemaResolver(schemaId);

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
                    types[schemaType.Key.Id] = schemaType.Value;
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
                    return types.GetOrDefault(content.Id);
                }

                return null;
            };
        }
    }
}
