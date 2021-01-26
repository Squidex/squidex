// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using GraphQL.Types;
using Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Contents;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types
{
    public sealed class AppQueriesGraphType : ObjectGraphType
    {
        public AppQueriesGraphType(IGraphModel model, IEnumerable<ISchemaEntity> schemas)
        {
            AddField(model.TypeFactory.FindAsset);
            AddField(model.TypeFactory.QueryAssets);
            AddField(model.TypeFactory.QueryAssetsWithTotal);

            foreach (var schema in schemas)
            {
                var schemaId = schema.Id;
                var schemaType = schema.TypeName();
                var schemaName = schema.DisplayName();

                var contentType = model.GetContentType(schema.Id);

                AddContentFind(
                    schemaId,
                    schemaType,
                    schemaName,
                    contentType);

                AddContentQueries(
                    schemaId,
                    schemaType,
                    schemaName,
                    contentType);
            }

            Description = "The app queries.";
        }

        private void AddContentFind(DomainId schemaId, string schemaType, string schemaName, IGraphType contentType)
        {
            AddField(new FieldType
            {
                Name = $"find{schemaType}Content",
                Arguments = ContentActions.Find.Arguments,
                ResolvedType = contentType,
                Resolver = ContentActions.Find.Resolver(schemaId),
                Description = $"Find an {schemaName} content by id."
            });
        }

        private void AddContentQueries(DomainId schemaId, string schemaType, string schemaName, IGraphType contentType)
        {
            var resolver = ContentActions.QueryOrReferencing.Query(schemaId);

            AddField(new FieldType
            {
                Name = $"query{schemaType}Contents",
                Arguments = ContentActions.QueryOrReferencing.Arguments,
                ResolvedType = new ListGraphType(new NonNullGraphType(contentType)),
                Resolver = resolver,
                Description = $"Query {schemaName} content items."
            });

            AddField(new FieldType
            {
                Name = $"query{schemaType}ContentsWithTotal",
                Arguments = ContentActions.QueryOrReferencing.Arguments,
                ResolvedType = new ContentsResultGraphType(schemaType, schemaName, contentType),
                Resolver = resolver,
                Description = $"Query {schemaName} content items with total count."
            });
        }
    }
}
