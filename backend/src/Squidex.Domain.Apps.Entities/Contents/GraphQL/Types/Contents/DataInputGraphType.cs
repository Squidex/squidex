// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL.Types;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Schemas;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Contents
{
    internal sealed class DataInputGraphType : InputObjectGraphType
    {
        public DataInputGraphType(Builder builder, SchemaInfo schemaInfo)
        {
            Name = schemaInfo.DataInputType;

            foreach (var fieldInfo in schemaInfo.Fields)
            {
                var resolvedType = builder.GetInputGraphType(fieldInfo);

                if (resolvedType != null)
                {
                    var fieldGraphType = new InputObjectGraphType
                    {
                        Name = fieldInfo.LocalizedInputType
                    };

                    var partitioning = builder.ResolvePartition(((RootField)fieldInfo.Field).Partitioning);

                    foreach (var partitionKey in partitioning.AllKeys)
                    {
                        fieldGraphType.AddField(new FieldType
                        {
                            Name = partitionKey.EscapePartition(),
                            ResolvedType = resolvedType,
                            Resolver = null,
                            Description = fieldInfo.Field.RawProperties.Hints
                        }).WithSourceName(partitionKey);
                    }

                    fieldGraphType.Description = $"The structure of the {fieldInfo.DisplayName} field of the {schemaInfo.DisplayName} content input type.";

                    AddField(new FieldType
                    {
                        Name = fieldInfo.FieldName,
                        ResolvedType = fieldGraphType,
                        Resolver = null
                    }).WithSourceName(fieldInfo);
                }
            }

            Description = $"The structure of the {schemaInfo.DisplayName} data input type.";
        }
    }
}
