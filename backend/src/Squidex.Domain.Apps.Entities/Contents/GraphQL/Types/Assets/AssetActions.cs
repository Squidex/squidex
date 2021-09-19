// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL;
using GraphQL.Resolvers;
using GraphQL.Types;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Assets
{
    internal static class AssetActions
    {
        public static class Metadata
        {
            public static readonly QueryArguments Arguments = new QueryArguments
            {
                new QueryArgument(AllTypes.String)
                {
                    Name = "path",
                    Description = FieldDescriptions.JsonPath,
                    DefaultValue = null
                }
            };

            public static readonly IFieldResolver Resolver = Resolvers.Sync<IEnrichedAssetEntity, object?>((source, fieldContext, _) =>
            {
                if (fieldContext.Arguments.TryGetValue("path", out var path))
                {
                    source.Metadata.TryGetByPath(path.Value as string, out var result);

                    return result;
                }

                return source.Metadata;
            });
        }

        public static class Find
        {
            public static readonly QueryArguments Arguments = new QueryArguments
            {
                new QueryArgument(AllTypes.NonNullString)
                {
                    Name = "id",
                    Description = "The id of the asset (usually GUID).",
                    DefaultValue = null
                }
            };

            public static readonly IFieldResolver Resolver = Resolvers.Async<object, object?>(async (_, fieldContext, context) =>
            {
                var assetId = fieldContext.GetArgument<DomainId>("id");

                return await context.FindAssetAsync(assetId,
                    fieldContext.CancellationToken);
            });
        }

        public static class Query
        {
            public static readonly QueryArguments Arguments = new QueryArguments
            {
                new QueryArgument(AllTypes.Int)
                {
                    Name = "top",
                    Description = "Optional number of assets to take.",
                    DefaultValue = null
                },
                new QueryArgument(AllTypes.Int)
                {
                    Name = "skip",
                    Description = "Optional number of assets to skip.",
                    DefaultValue = 0
                },
                new QueryArgument(AllTypes.String)
                {
                    Name = "filter",
                    Description = "Optional OData filter.",
                    DefaultValue = null
                },
                new QueryArgument(AllTypes.String)
                {
                    Name = "orderby",
                    Description = "Optional OData order definition.",
                    DefaultValue = null
                }
            };

            public static readonly IFieldResolver Resolver = Resolvers.Async<object, object>(async (_, fieldContext, context) =>
            {
                var query = fieldContext.BuildODataQuery();

                var q = Q.Empty.WithODataQuery(query).WithoutTotal();

                return await context.QueryAssetsAsync(q,
                    fieldContext.CancellationToken);
            });

            public static readonly IFieldResolver ResolverWithTotal = Resolvers.Async<object, object>(async (_, fieldContext, context) =>
            {
                var query = fieldContext.BuildODataQuery();

                var q = Q.Empty.WithODataQuery(query);

                return await context.QueryAssetsAsync(q,
                    fieldContext.CancellationToken);
            });
        }
    }
}
