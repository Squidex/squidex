// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using GraphQL.Resolvers;
using GraphQL.Types;
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
                var fieldInfo = model.GetGraphType(field);

                if (fieldInfo.ResolveType != null)
                {
                    var fieldName = field.RawProperties.Label.WithFallback(field.Name);

                    var fieldGraphType = new ObjectGraphType
                    {
                        Name = $"{schemaType}Data{field.Name.ToPascalCase()}Dto"
                    };

                    var partition = model.ResolvePartition(field.Partitioning);

                    foreach (var partitionItem in partition)
                    {
                        fieldGraphType.AddField(new FieldType
                        {
                            Name = partitionItem.Key,
                            Resolver = fieldInfo.Resolver,
                            ResolvedType = fieldInfo.ResolveType,
                            Description = field.RawProperties.Hints
                        });
                    }

                    fieldGraphType.Description = $"The structure of the {fieldName} of a {schemaName} content type.";

                    var fieldResolver = new FuncFieldResolver<NamedContentData, ContentFieldData>(c => c.Source.GetOrDefault(field.Name));

                    AddField(new FieldType
                    {
                        Name = field.Name.ToCamelCase(),
                        Resolver = fieldResolver,
                        ResolvedType = fieldGraphType,
                        Description = $"The {fieldName} field."
                    });
                }
            }

            Description = $"The structure of a {schemaName} content type.";
        }
    }
}
