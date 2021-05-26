﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL.Types;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Contents
{
    internal sealed class DataFlatGraphType : ObjectGraphType<FlatContentData>
    {
        public DataFlatGraphType(Builder builder, SchemaInfo schemaInfo)
        {
            Name = schemaInfo.DataFlatType;

            foreach (var fieldInfo in schemaInfo.Fields)
            {
                if (fieldInfo.Field.RawProperties is ComponentFieldProperties ||
                    fieldInfo.Field.RawProperties is ComponentsFieldProperties)
                {
                    AddField(new FieldType
                    {
                        Name = fieldInfo.FieldNameDynamic,
                        Arguments = ContentActions.Json.Arguments,
                        ResolvedType = AllTypes.Json,
                        Resolver = FieldVisitor.JsonPath,
                        Description = fieldInfo.Field.RawProperties.Hints
                    }).WithSourceName(fieldInfo);
                }
                else
                {
                    var (resolvedType, resolver, args) = builder.GetGraphType(fieldInfo);

                    if (resolver != null)
                    {
                        AddField(new FieldType
                        {
                            Name = fieldInfo.FieldName,
                            Arguments = args,
                            ResolvedType = resolvedType,
                            Resolver = resolver,
                            Description = fieldInfo.Field.RawProperties.Hints
                        }).WithSourceName(fieldInfo);
                    }
                }
            }

            Description = $"The structure of the flat {schemaInfo.DisplayName} data type.";
        }
    }
}
