// ==========================================================================
//  SchemaGraphType.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using GraphQL.Resolvers;
using GraphQL.Types;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure;
using Schema = Squidex.Domain.Apps.Core.Schemas.Schema;

namespace Squidex.Domain.Apps.Read.Contents.GraphQL.Types
{
    public sealed class ContentGraphType : ObjectGraphType<IContentEntity>
    {
        private static readonly IFieldResolver DataResolver =
            new FuncFieldResolver<IContentEntity, NamedContentData>(c => c.Source.Data);

        public ContentGraphType(Schema schema, IGraphQLContext graphQLContext)
        {
            var schemaName = schema.Properties.Label.WithFallback(schema.Name);

            Name = $"{schema.Name.ToPascalCase()}Dto";

            Field("id", x => x.Id.ToString())
                .Description($"The id of the {schemaName} content.");

            Field("version", x => x.Version)
                .Description($"The version of the {schemaName} content.");

            Field("created", x => x.Created.ToDateTimeUtc())
                .Description($"The date and time when the {schemaName} content has been created.");

            Field("createdBy", x => x.CreatedBy.ToString())
                .Description($"The user that has created the {schemaName} content.");

            Field("lastModified", x => x.LastModified.ToDateTimeUtc())
                .Description($"The date and time when the {schemaName} content has been modified last.");

            Field("lastModifiedBy", x => x.LastModifiedBy.ToString())
                .Description($"The user that has updated the {schemaName} content last.");
            
            AddField(new FieldType
            {
                Name = "data",
                Resolver = DataResolver,
                ResolvedType = new NonNullGraphType(new ContentDataGraphType(schema, graphQLContext)),
                Description = $"The data of the {schemaName} content."
            });

            Description = $"The structure of a {schemaName} content type.";
        }
    }
}
