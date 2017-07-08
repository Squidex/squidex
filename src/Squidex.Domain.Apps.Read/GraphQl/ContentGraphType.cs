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
using Squidex.Domain.Apps.Read.Contents;
using Squidex.Infrastructure;
using Schema = Squidex.Domain.Apps.Core.Schemas.Schema;

namespace Squidex.Domain.Apps.Read.GraphQl
{
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
}
