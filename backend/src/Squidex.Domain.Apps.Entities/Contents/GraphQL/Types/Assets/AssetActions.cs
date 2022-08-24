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
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Core.Subscriptions;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Infrastructure;
using Squidex.Messaging.Subscriptions;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Assets
{
    internal static class AssetActions
    {
        public static class Metadata
        {
            public static readonly QueryArguments Arguments = new QueryArguments
            {
                new QueryArgument(Scalars.String)
                {
                    Name = "path",
                    Description = FieldDescriptions.JsonPath,
                    DefaultValue = null
                }
            };

            public static readonly IFieldResolver Resolver = Resolvers.Sync<IEnrichedAssetEntity, object?>((source, fieldContext, _) =>
            {
                if (fieldContext.Arguments != null &&
                    fieldContext.Arguments.TryGetValue("path", out var path))
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
                new QueryArgument(Scalars.NonNullString)
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
                new QueryArgument(Scalars.Int)
                {
                    Name = "top",
                    Description = FieldDescriptions.QueryTop,
                    DefaultValue = null
                },
                new QueryArgument(Scalars.Int)
                {
                    Name = "skip",
                    Description = FieldDescriptions.QuerySkip,
                    DefaultValue = 0
                },
                new QueryArgument(Scalars.String)
                {
                    Name = "filter",
                    Description = FieldDescriptions.QueryFilter,
                    DefaultValue = null
                },
                new QueryArgument(Scalars.String)
                {
                    Name = "orderby",
                    Description = FieldDescriptions.QueryOrderBy,
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

        public static class Subscription
        {
            public static readonly QueryArguments Arguments = new QueryArguments
            {
                new QueryArgument(Scalars.EnrichedAssetEventType)
                {
                    Name = "type",
                    Description = FieldDescriptions.EventType,
                    DefaultValue = null
                }
            };

            public static readonly ISourceStreamResolver Resolver = Resolvers.Stream((fieldContext, context) =>
            {
                var type = fieldContext.GetArgument<EnrichedAssetEventType?>("type");

                var subscription = new AssetSubscription
                {
                    Type = type,
                    // The app id is taken from the URL so we cannot get events from other apps.
                    AppId = context.Context.App.Id
                };

                return context.Resolve<ISubscriptionService>().Subscribe<object, AssetSubscription>(subscription);
            });
        }
    }
}
