// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL.Types;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Schemas;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Contents;

internal sealed class DataGraphType : ObjectGraphType<ContentData>
{
    public DataGraphType(Builder builder, SchemaInfo schemaInfo)
    {
        // The name is used for equal comparison. Therefore it is important to treat it as readonly.
        Name = schemaInfo.DataType;

        foreach (var fieldInfo in schemaInfo.Fields)
        {
            var partitioning = builder.ResolvePartition(((RootField)fieldInfo.Field).Partitioning);

            if (fieldInfo.Field.IsComponentLike())
            {
                var fieldGraphType = new ObjectGraphType
                {
                    // The name is used for equal comparison. Therefore it is important to treat it as readonly.
                    Name = fieldInfo.LocalizedTypeDynamic
                };

                foreach (var partitionKey in partitioning.AllKeys)
                {
                    fieldGraphType.AddField(new FieldType
                    {
                        Name = partitionKey.EscapePartition(),
                        Arguments = ContentActions.Json.Arguments,
                        ResolvedType = Scalars.Json,
                        Resolver = FieldVisitor.JsonPath,
                        Description = fieldInfo.Field.RawProperties.Hints
                    }).WithSourceName(partitionKey);
                }

                fieldGraphType.Description = $"The dynamic structure of the {fieldInfo.DisplayName} field of the {schemaInfo.DisplayName} content type.";

                AddField(new FieldType
                {
                    Name = fieldInfo.FieldNameDynamic,
                    ResolvedType = fieldGraphType,
                    Resolver = ContentResolvers.Field
                }).WithSourceName(fieldInfo);
            }

            var (resolvedType, resolver, args) = builder.GetGraphType(fieldInfo);

            if (resolver != null)
            {
                var fieldGraphType = new ObjectGraphType
                {
                    // The name is used for equal comparison. Therefore it is important to treat it as readonly.
                    Name = fieldInfo.LocalizedType
                };

                foreach (var partitionKey in partitioning.AllKeys)
                {
                    fieldGraphType.AddField(new FieldType
                    {
                        Name = partitionKey.EscapePartition(),
                        Arguments = args,
                        ResolvedType = resolvedType,
                        Resolver = resolver,
                        Description = fieldInfo.Field.RawProperties.Hints
                    }).WithSourceName(partitionKey);
                }

                fieldGraphType.Description = $"The structure of the {fieldInfo.DisplayName} field of the {schemaInfo.DisplayName} content type.";

                AddField(new FieldType
                {
                    Name = fieldInfo.FieldName,
                    ResolvedType = fieldGraphType,
                    Resolver = ContentResolvers.Field
                }).WithSourceName(fieldInfo);
            }
        }

        Description = $"The structure of the {schemaInfo.DisplayName} data type.";
    }
}
