// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using GraphQL.Resolvers;
using GraphQL.Types;
using Newtonsoft.Json.Linq;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types
{
    public sealed class ContentDataGraphType : ObjectGraphType<NamedContentData>
    {
        public void Initialize(IGraphModel model, ISchemaEntity schema)
        {
            var schemaType = schema.TypeName();
            var schemaName = schema.DisplayName();

            Name = $"{schemaType}DataDto";

            foreach (var field in schema.SchemaDef.Fields.Where(x => !x.IsHidden))
            {
                var fieldInfo = model.GetGraphType(schema, field);

                if (fieldInfo.ResolveType != null)
                {
                    var fieldType = field.TypeName();
                    var fieldName = field.DisplayName();

                    var fieldGraphType = new ObjectGraphType
                    {
                        Name = $"{schemaType}Data{fieldType}Dto"
                    };

                    var partition = model.ResolvePartition(field.Partitioning);

                    foreach (var partitionItem in partition)
                    {
                        var resolver = new FuncFieldResolver<object>(c =>
                        {
                            if (((ContentFieldData)c.Source).TryGetValue(c.FieldName, out var value))
                            {
                                return fieldInfo.Resolver(value, c);
                            }
                            else
                            {
                                return fieldInfo;
                            }
                        });

                        fieldGraphType.AddField(new FieldType
                        {
                            Name = partitionItem.Key,
                            Resolver = resolver,
                            ResolvedType = fieldInfo.ResolveType,
                            Description = field.RawProperties.Hints
                        });
                    }

                    fieldGraphType.Description = $"The structure of the {fieldName} field of the {schemaName} content type.";

                    var fieldResolver = new FuncFieldResolver<NamedContentData, IReadOnlyDictionary<string, JToken>>(c =>
                    {
                        return c.Source.GetOrDefault(field.Name);
                    });

                    AddField(new FieldType
                    {
                        Name = field.Name.ToCamelCase(),
                        Resolver = fieldResolver,
                        ResolvedType = fieldGraphType,
                        Description = $"The {fieldName} field."
                    });
                }
            }

            Description = $"The structure of the {schemaName} content type.";
        }
    }
}
