// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL.Types;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Contents
{
    internal sealed class NestedGraphType : ObjectGraphType<JsonObject>
    {
        public NestedGraphType(Builder builder, FieldInfo fieldInfo)
        {
            // The name is used for equal comparison. Therefore it is important to treat it as readonly.
            Name = fieldInfo.NestedType;

            foreach (var nestedFieldInfo in fieldInfo.Fields)
            {
                if (nestedFieldInfo.Field.IsComponentLike())
                {
                    AddField(new FieldType
                    {
                        Name = nestedFieldInfo.FieldNameDynamic,
                        Arguments = ContentActions.Json.Arguments,
                        ResolvedType = AllTypes.Json,
                        Resolver = FieldVisitor.JsonPath,
                        Description = nestedFieldInfo.Field.RawProperties.Hints
                    }).WithSourceName(nestedFieldInfo);
                }

                var (resolvedType, resolver, args) = builder.GetGraphType(nestedFieldInfo);

                if (resolvedType != null && resolver != null)
                {
                    AddField(new FieldType
                    {
                        Name = nestedFieldInfo.FieldName,
                        Arguments = args,
                        ResolvedType = resolvedType,
                        Resolver = resolver,
                        Description = nestedFieldInfo.Field.RawProperties.Hints
                    }).WithSourceName(nestedFieldInfo);
                }
            }

            Description = $"The structure of the {fieldInfo.DisplayName} nested schema.";
        }
    }
}
