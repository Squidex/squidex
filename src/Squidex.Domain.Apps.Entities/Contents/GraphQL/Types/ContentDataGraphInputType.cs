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
    public sealed class ContentDataGraphInputType : InputObjectGraphType
    {
        public ContentDataGraphInputType(IGraphModel model, ISchemaEntity schema)
        {
            var schemaType = schema.TypeName();
            var schemaName = schema.DisplayName();

            Name = $"{schemaType}InputDto";

            foreach (var field in schema.SchemaDef.Fields.Where(x => !x.IsHidden))
            {
                var inputType = model.GetInputGraphType(field);

                if (inputType != null)
                {
                    if (field.RawProperties.IsRequired)
                    {
                        inputType = new NonNullGraphType(inputType);
                    }

                    var fieldName = field.RawProperties.Label.WithFallback(field.Name);

                    var fieldGraphType = new InputObjectGraphType
                    {
                        Name = $"{schemaType}Data{field.Name.ToPascalCase()}InputDto"
                    };

                    var partition = model.ResolvePartition(field.Partitioning);

                    foreach (var partitionItem in partition)
                    {
                        fieldGraphType.AddField(new FieldType
                        {
                            Name = partitionItem.Key,
                            ResolvedType = inputType,
                            Resolver = null,
                            Description = field.RawProperties.Hints
                        });
                    }

                    fieldGraphType.Description = $"The input structure of the {fieldName} of a {schemaName} content type.";

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
