// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using GraphQL.Types;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Primitives;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Contents
{
    internal sealed class DataInputGraphType : InputObjectGraphType
    {
        public DataInputGraphType(Builder builder, SchemaInfo schemaInfo)
        {
            // The name is used for equal comparison. Therefore it is important to treat it as readonly.
            Name = schemaInfo.DataInputType;

            foreach (var fieldInfo in schemaInfo.Fields)
            {
                var resolvedType = builder.GetInputGraphType(fieldInfo);

                if (resolvedType != null)
                {
                    var fieldGraphType = new InputObjectGraphType
                    {
                        // The name is used for equal comparison. Therefore it is important to treat it as readonly.
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

        public override object ParseDictionary(IDictionary<string, object> value)
        {
            var result = new ContentData();

            static ContentFieldData ToFieldData(IDictionary<string, object> source, IComplexGraphType type)
            {
                var result = new ContentFieldData();

                foreach (var field in type.Fields)
                {
                    if (source.TryGetValue(field.Name, out var value))
                    {
                        if (value is IEnumerable<object> list && field.ResolvedType.Flatten() is IComplexGraphType nestedType)
                        {
                            var array = new JsonArray(list.Count());

                            foreach (var item in list)
                            {
                                if (item is JsonObject nested)
                                {
                                    array.Add(nested);
                                }
                            }

                            result[field.SourceName()] = array;
                        }
                        else
                        {
                            result[field.SourceName()] = JsonGraphType.ParseJson(value);
                        }
                    }
                }

                return result;
            }

            foreach (var field in Fields)
            {
                if (field.ResolvedType is IComplexGraphType complexType && value.TryGetValue(field.Name, out var fieldValue) && fieldValue is IDictionary<string, object> nested)
                {
                    result[field.SourceName()] = ToFieldData(nested, complexType);
                }
            }

            return result;
        }
    }
}
