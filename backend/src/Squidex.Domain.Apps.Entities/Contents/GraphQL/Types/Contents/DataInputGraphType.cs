// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL.Types;
using Squidex.Domain.Apps.Core.Schemas;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Contents;

internal sealed class DataInputGraphType : InputObjectGraphType
{
    private readonly FieldMap fieldMap;

    public DataInputGraphType(Builder builder, SchemaInfo schemaInfo)
    {
        // The name is used for equal comparison. Therefore it is important to treat it as readonly.
        Name = schemaInfo.DataInputType;

        foreach (var fieldInfo in schemaInfo.Fields)
        {
            var resolvedType = builder.GetInputGraphType(fieldInfo);

            if (resolvedType == null)
            {
                continue;
            }

            var fieldGraphType = new InputObjectGraphType
            {
                // The name is used for equal comparison. Therefore it is important to treat it as readonly.
                Name = fieldInfo.LocalizedInputType
            };

            var partitioning = builder.ResolvePartition(((RootField)fieldInfo.Field).Partitioning);

            foreach (var partitionKey in partitioning.AllKeys)
            {
                fieldGraphType.AddField(new FieldTypeWithSourceName
                {
                    Name = partitionKey.EscapePartition(),
                    ResolvedType = resolvedType,
                    Resolver = null,
                    Description = fieldInfo.Field.RawProperties.Hints,
                    SourceName = partitionKey
                });
            }

            fieldGraphType.Description = $"The structure of the {fieldInfo.DisplayName} field of the {schemaInfo.DisplayName} content input type.";

            AddField(new FieldTypeWithSourceName
            {
                Name = fieldInfo.FieldName,
                ResolvedType = fieldGraphType,
                Resolver = null,
                SourceName = fieldInfo.Field.Name,
            });
        }

        Description = $"The structure of the {schemaInfo.DisplayName} data input type.";

        fieldMap = builder.FieldMap;
    }

    public override object ParseDictionary(IDictionary<string, object?> value)
    {
        return fieldMap.MapData(this, value);
    }
}
