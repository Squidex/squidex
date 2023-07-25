// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL.Types;
using Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Contents;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types;

internal sealed class ApplicationQueries : ObjectGraphType
{
    public ApplicationQueries(Builder builder, IEnumerable<SchemaInfo> schemaInfos)
    {
        AddField(SharedTypes.FindAsset);
        AddField(SharedTypes.QueryAssets);
        AddField(SharedTypes.QueryAssetsWithTotal);
        AddContentQuery(builder);

        foreach (var schemaInfo in schemaInfos)
        {
            var contentType = builder.GetContentType(schemaInfo);

            if (contentType == null)
            {
                continue;
            }

            AddContentFind(schemaInfo, contentType);
            AddContentQueries(builder, schemaInfo, contentType);
        }

        Description = "The app queries.";
    }

    private void AddContentFind(SchemaInfo schemaInfo, IGraphType contentType)
    {
        AddField(new FieldType
        {
            Name = $"find{schemaInfo.TypeName}Content",
            Arguments = ContentActions.Find.Arguments,
            ResolvedType = contentType,
            Resolver = ContentActions.Find.Resolver,
            Description = $"Find an {schemaInfo.DisplayName} content by id."
        }).WithSchemaId(schemaInfo);
    }

    private void AddContentQueries(Builder builder, SchemaInfo schemaInfo, IGraphType contentType)
    {
        AddField(new FieldType
        {
            Name = $"query{schemaInfo.TypeName}Contents",
            Arguments = ContentActions.QueryOrReferencing.Arguments,
            ResolvedType = new ListGraphType(new NonNullGraphType(contentType)),
            Resolver = ContentActions.QueryOrReferencing.Query,
            Description = $"Query {schemaInfo.DisplayName} content items."
        }).WithSchemaId(schemaInfo);

        var contentResultTyp = builder.GetContentResultType(schemaInfo);

        if (contentResultTyp == null)
        {
            return;
        }

        AddField(new FieldType
        {
            Name = $"query{schemaInfo.TypeName}ContentsWithTotal",
            Arguments = ContentActions.QueryOrReferencing.Arguments,
            ResolvedType = contentResultTyp,
            Resolver = ContentActions.QueryOrReferencing.QueryWithTotal,
            Description = $"Query {schemaInfo.DisplayName} content items with total count."
        }).WithSchemaId(schemaInfo);
    }

    private void AddContentQuery(Builder builder)
    {
        var unionType = builder.GetContentUnion("AllContents", null);

        if (unionType.SchemaTypes.Count == 0)
        {
            return;
        }

        AddField(new FieldType
        {
            Name = "queryContentsByIds",
            Arguments = ContentActions.QueryByIds.Arguments,
            ResolvedType = new NonNullGraphType(new ListGraphType(new NonNullGraphType(unionType))),
            Resolver = ContentActions.QueryByIds.Resolver,
            Description = "Query content items by IDs across schemeas."
        });
    }
}
