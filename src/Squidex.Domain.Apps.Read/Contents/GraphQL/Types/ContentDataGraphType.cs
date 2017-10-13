// ==========================================================================
//  ContentDataGraphType.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Linq;
using GraphQL.Resolvers;
using GraphQL.Types;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure;
using Schema = Squidex.Domain.Apps.Core.Schemas.Schema;

namespace Squidex.Domain.Apps.Read.Contents.GraphQL.Types
{
    public sealed class ContentDataGraphType : ObjectGraphType<NamedContentData>
    {
        public ContentDataGraphType(Schema schema, IGraphQLContext context)
        {
            var schemaName = schema.Properties.Label.WithFallback(schema.Name);

            Name = $"{schema.Name.ToPascalCase()}DataDto";

            foreach (var field in schema.Fields.Where(x => !x.IsHidden))
            {
                var fieldInfo = context.GetGraphType(field);

                if (fieldInfo.ResolveType != null)
                {
                    var fieldName = field.RawProperties.Label.WithFallback(field.Name);

                    var fieldGraphType = new ObjectGraphType
                    {
                        Name = $"{schema.Name.ToPascalCase()}Data{field.Name.ToPascalCase()}Dto"
                    };

                    var partition = context.ResolvePartition(field.Partitioning);

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
                        ResolvedType = fieldGraphType
                    });
                }
            }

            Description = $"The structure of a {schemaName} content type.";
        }
    }
}
