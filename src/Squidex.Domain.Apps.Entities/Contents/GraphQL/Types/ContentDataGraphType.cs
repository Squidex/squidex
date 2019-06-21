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
        public void Initialize(IGraphModel model, ISchemaEntity schema)
        {
            var schemaType = schema.TypeName();
            var schemaName = schema.DisplayName();

            Name = $"{schemaType}DataDto";

            foreach (var (field, fieldName, typeName) in schema.SchemaDef.Fields.SafeFields())
            {
                var (resolvedType, valueResolver) = model.GetGraphType(schema, field, fieldName);

                if (valueResolver != null)
                {
                    var displayName = field.DisplayName();

                    var fieldGraphType = new ObjectGraphType
                    {
                        Name = $"{schemaType}Data{typeName}Dto"
                    };

                    var partition = model.ResolvePartition(field.Partitioning);

                    foreach (var partitionItem in partition)
                    {
                        var key = partitionItem.Key;

                        fieldGraphType.AddField(new FieldType
                        {
                            Name = key.EscapePartition(),
                            Resolver = PartitionResolver(valueResolver, key),
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

            Description = $"The structure of the {schemaName} content type.";
        }

        private static FuncFieldResolver<object> PartitionResolver(ValueResolver valueResolver, string key)
        {
            return new FuncFieldResolver<object>(c =>
            {
                if (((ContentFieldData)c.Source).TryGetValue(key, out var value))
                {
                    return valueResolver(value, c);
                }
                else
                {
                    return null;
                }
            });
        }

        private static FuncFieldResolver<NamedContentData, IReadOnlyDictionary<string, IJsonValue>> FieldResolver(RootField field)
        {
            return new FuncFieldResolver<NamedContentData, IReadOnlyDictionary<string, IJsonValue>>(c =>
            {
                return c.Source.GetOrDefault(field.Name);
            });
        }
    }
}
