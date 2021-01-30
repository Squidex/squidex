// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL.Types;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Schemas;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Contents
{
    internal sealed class DataGraphType : ObjectGraphType<ContentData>
    {
        public DataGraphType(Builder builder, SchemaInfo schemaInfo)
        {
            Name = schemaInfo.DataType;

            foreach (var fieldInfo in schemaInfo.Fields)
            {
                var (resolvedType, resolver, args) = builder.GetGraphType(fieldInfo);

                if (resolver != null)
                {
                    var fieldGraphType = new ObjectGraphType
                    {
                        Name = fieldInfo.LocalizedType
                    };

                    var partitioning = builder.ResolvePartition(((RootField)fieldInfo.Field).Partitioning);

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
}
