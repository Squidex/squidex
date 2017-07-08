// ==========================================================================
//  SchemaGraphType.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using GraphQL.Resolvers;
using GraphQL.Types;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Read.Contents;
using Squidex.Infrastructure;
using Schema = Squidex.Domain.Apps.Core.Schemas.Schema;

namespace Squidex.Domain.Apps.Read.GraphQl
{
    public interface IGraphQLContext : IGraphQLResolver
    {
        IFieldPartitioning ResolvePartition(Partitioning key);
    }

    public sealed class ContentGraphType : ObjectGraphType<IContentEntity>
    {
        private static readonly IFieldResolver DataResolver =
            new FuncFieldResolver<IContentEntity, NamedContentData>(c => c.Source.Data);

        public ContentGraphType(Schema schema, IGraphQLContext context)
        {
            var schemaName = schema.Properties.Label.WithFallback(schema.Name);

            Field("id", x => x.Id)
                .Description($"The id of the {schemaName} content.");

            Field("version", x => x.Version)
                .Description($"The version of the {schemaName} content.");

            Field("created", x => x.Created)
                .Description($"The date and time when the {schemaName} content has been created.");

            Field("createdBy", x => x.CreatedBy.ToString())
                .Description($"The user that has created the {schemaName} content.");

            Field("lastModified", x => x.LastModified.ToString())
                .Description($"The date and time when the {schemaName} content has been modified last.");

            Field("lastModified", x => x.LastModified.ToString())
                .Description($"The user that has updated the {schemaName} content last.");

            AddField(new FieldType
            {
                Name = "data",
                Resolver = DataResolver,
                ResolvedType = new SchemaDataGraphType(schema, context),
                Description = $"The version of the {schemaName} content."
            });
        }
    }

    public sealed class SchemaDataGraphType : ObjectGraphType<NamedContentData>
    {
        private static readonly IFieldResolver FieldResolver = 
            new FuncFieldResolver<NamedContentData, ContentFieldData>(c => c.Source.GetOrDefault(c.FieldName));

        public SchemaDataGraphType(Schema schema, IGraphQLContext context)
        {
            foreach (var field in schema.Fields)
            {
                var fieldName = field.Name;

                AddField(new FieldType
                {
                    Name = fieldName,
                    Resolver = FieldResolver,
                    ResolvedType = new SchemaFieldGraphType(field, context),
                });
            }
        }
    }

    public sealed class SchemaFieldGraphType : ObjectGraphType<ContentFieldData>
    {
        public SchemaFieldGraphType(Field field, IGraphQLContext context)
        {
            var partition = context.ResolvePartition(field.Paritioning);

            foreach (var partitionItem in partition)
            {
                AddField(new FieldType
                {
                    Name = partitionItem.Key,
                    Resolver = new FuncFieldResolver<object>()
                }
            }
        }
    }
}
