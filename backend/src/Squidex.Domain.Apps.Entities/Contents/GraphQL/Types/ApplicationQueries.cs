// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL.Types;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Contents;
using static Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Contents.ContentActions;

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

            if (schemaInfo.Schema.Type == SchemaType.Singleton)
            {
                // Mark the normal queries as deprecated to motivate using the new endpoint.
                var deprecation = $"Use 'find{schemaInfo.TypeName}Singleton' instead.";

                AddContentFind(schemaInfo, contentType, deprecation);
                AddContentFindSingleton(schemaInfo, contentType);
                AddContentQueries(builder, schemaInfo, contentType, deprecation);
            }
            else
            {
                AddContentFind(schemaInfo, contentType, null);
                AddContentQueries(builder, schemaInfo, contentType, null);
            }
        }

        Description = "The app queries.";
    }

    private void AddContentFind(SchemaInfo schemaInfo, IGraphType contentType, string? deprecatedReason)
    {
        AddField(new FieldTypeWithSchemaId
        {
            Name = $"find{schemaInfo.TypeName}Content",
            Arguments = Find.Arguments,
            ResolvedType = contentType,
            Resolver = Find.Resolver,
            DeprecationReason = deprecatedReason,
            Description = $"Find an {schemaInfo.DisplayName} content by id.",
            SchemaId = schemaInfo.Schema.Id
        });
    }

    private void AddContentFindSingleton(SchemaInfo schemaInfo, IGraphType contentType)
    {
        AddField(new FieldTypeWithSchemaId
        {
            Name = $"find{schemaInfo.TypeName}Singleton",
            Arguments = FindSingleton.Arguments,
            ResolvedType = contentType,
            Resolver = FindSingleton.Resolver,
            DeprecationReason = null,
            Description = $"Find an {schemaInfo.DisplayName} singleton.",
            SchemaId = schemaInfo.Schema.Id
        });
    }

    private void AddContentQueries(Builder builder, SchemaInfo schemaInfo, IGraphType contentType, string? deprecatedReason)
    {
        AddField(new FieldTypeWithSchemaId
        {
            Name = $"query{schemaInfo.TypeName}Contents",
            Arguments = QueryOrReferencing.Arguments,
            ResolvedType = new ListGraphType(new NonNullGraphType(contentType)),
            Resolver = QueryOrReferencing.Query,
            DeprecationReason = deprecatedReason,
            Description = $"Query {schemaInfo.DisplayName} content items.",
            SchemaId = schemaInfo.Schema.Id
        });

        var contentResultTyp = builder.GetContentResultType(schemaInfo);

        if (contentResultTyp == null)
        {
            return;
        }

        AddField(new FieldTypeWithSchemaId
        {
            Name = $"query{schemaInfo.TypeName}ContentsWithTotal",
            Arguments = QueryOrReferencing.Arguments,
            ResolvedType = contentResultTyp,
            Resolver = QueryOrReferencing.QueryWithTotal,
            DeprecationReason = deprecatedReason,
            Description = $"Query {schemaInfo.DisplayName} content items with total count.",
            SchemaId = schemaInfo.Schema.Id
        });
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
            Arguments = QueryByIds.Arguments,
            ResolvedType = new NonNullGraphType(new ListGraphType(new NonNullGraphType(unionType))),
            Resolver = QueryByIds.Resolver,
            Description = "Query content items by IDs across schemeas."
        });
    }
}
