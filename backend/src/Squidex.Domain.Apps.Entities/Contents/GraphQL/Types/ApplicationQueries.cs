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

        foreach (var schemaInfo in schemaInfos)
        {
            var contentType = builder.GetContentType(schemaInfo);

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

        var resultType = builder.GetContentResultType(schemaInfo);

        AddField(new FieldType
        {
            Name = $"query{schemaInfo.TypeName}ContentsWithTotal",
            Arguments = ContentActions.QueryOrReferencing.Arguments,
            ResolvedType = resultType,
            Resolver = ContentActions.QueryOrReferencing.QueryWithTotal,
            Description = $"Query {schemaInfo.DisplayName} content items with total count."
        }).WithSchemaId(schemaInfo);
    }
}
