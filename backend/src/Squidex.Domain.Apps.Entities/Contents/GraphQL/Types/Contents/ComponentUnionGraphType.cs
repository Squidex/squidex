﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using GraphQL.Types;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Contents
{
    internal sealed class ComponentUnionGraphType : UnionGraphType
    {
        private readonly Dictionary<string, IObjectGraphType> types = new Dictionary<string, IObjectGraphType>();

        public bool HasType => types.Count > 0;

        public ComponentUnionGraphType(Builder builder, FieldInfo fieldInfo, ImmutableList<DomainId>? schemaIds)
        {
            // The name is used for equal comparison. Therefore it is important to treat it as readonly.
            Name = fieldInfo.ReferenceType;

            if (schemaIds?.Any() == true)
            {
                foreach (var schemaId in schemaIds)
                {
                    var contentType = builder.GetComponentType(schemaId);

                    if (contentType != null)
                    {
                        types[schemaId.ToString()] = contentType;
                    }
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
                    if (value is JsonObject component && component.TryGetValue<JsonString>(Component.Discriminator, out var schemaId))
                    {
                        return types.GetOrDefault(schemaId.Value);
                    }

                    return null;
                };
            }
        }
    }
}
