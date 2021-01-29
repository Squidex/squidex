// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL.Types;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Contents
{
    public sealed class NestedGraphType : ObjectGraphType<JsonObject>
    {
        public NestedGraphType(GraphQLModel model, FieldInfo fieldInfo)
        {
            Name = fieldInfo.NestedType;

            foreach (var nestedFieldInfo in fieldInfo.Fields)
            {
                var (resolvedType, resolver, args) = model.GetGraphType(nestedFieldInfo);

                if (resolvedType != null && resolver != null)
                {
                    AddField(new FieldType
                    {
                        Name = nestedFieldInfo.FieldName,
                        Arguments = args,
                        ResolvedType = resolvedType,
                        Resolver = resolver,
                        Description = $"The {fieldInfo.DisplayName}/{nestedFieldInfo.DisplayName} nested field."
                    }).WithSourceName(nestedFieldInfo);
                }
            }

            Description = $"The structure of the {fieldInfo.DisplayName} nested schema.";
        }
    }
}
