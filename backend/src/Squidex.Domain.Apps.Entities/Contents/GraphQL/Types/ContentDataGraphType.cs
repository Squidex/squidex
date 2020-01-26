// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using GraphQL.Resolvers;
using GraphQL.Types;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types
{
    public sealed class ContentDataGraphType : ObjectGraphType<NamedContentData>
    {
        public ContentDataGraphType(ISchemaEntity schema, string schemaName, string schemaType, IGraphModel model)
        {
            Name = $"{schemaType}DataDto";

            foreach (var (field, fieldName, typeName) in schema.SchemaDef.Fields.SafeFields())
            {
                var (resolvedType, valueResolver, args) = model.GetGraphType(schema, field, fieldName);

                if (valueResolver != null)
                {
                    var displayName = field.DisplayName();

                    var fieldGraphType = new ObjectGraphType
                    {
                        Name = $"{schemaType}Data{typeName}Dto"
                    };

                    var partitioning = model.ResolvePartition(field.Partitioning);

                    foreach (var partitionKey in partitioning.AllKeys)
                    {
                        fieldGraphType.AddField(new FieldType
                        {
                            Name = partitionKey.EscapePartition(),
                            Arguments = args,
                            Resolver = PartitionResolver(valueResolver, partitionKey),
                            ResolvedType = resolvedType,
                            Description = field.RawProperties.Hints
                        });
                    }

                    fieldGraphType.Description = $"The structure of the {displayName} field of the {schemaName} content type.";

                    AddField(new FieldType
                    {
                        Name = fieldName,
                        Resolver = FieldResolver(field),
                        ResolvedType = fieldGraphType,
                        Description = $"The {displayName} field."
                    });
                }
            }

            Description = $"The structure of the {schemaName} data type.";
        }

        private static FuncFieldResolver<object?> PartitionResolver(ValueResolver valueResolver, string key)
        {
            return new FuncFieldResolver<object?>(c =>
            {
                var source = (ContentFieldData)c.Source;

                if (source.TryGetValue(key, out var value) && value != null)
                {
                    return valueResolver(value, c);
                }
                else
                {
                    return null;
                }
            });
        }

        private static FuncFieldResolver<NamedContentData, IReadOnlyDictionary<string, IJsonValue>?> FieldResolver(RootField field)
        {
            return new FuncFieldResolver<NamedContentData, IReadOnlyDictionary<string, IJsonValue>?>(c =>
            {
                return c.Source?.GetOrDefault(field.Name);
            });
        }
    }
}
