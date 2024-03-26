// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Reactive.Linq;
using GraphQL;
using GraphQL.Resolvers;
using GraphQL.Types;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Core.Subscriptions;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Translations;
using Squidex.Messaging.Subscriptions;
using Squidex.Shared;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Assets;

internal static class AssetActions
{
    public static class Metadata
    {
        public static readonly QueryArguments Arguments =
        [
            new QueryArgument(Scalars.String)
            {
                Name = "path",
                Description = FieldDescriptions.JsonPath,
                DefaultValue = null
            },
        ];

        public static readonly IFieldResolver Resolver = Resolvers.Sync<EnrichedAsset, object?>((source, fieldContext, _) =>
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
        public static readonly QueryArguments Arguments =
        [
            new QueryArgument(Scalars.NonNullString)
            {
                Name = "id",
                Description = "The ID of the asset (usually GUID).",
                DefaultValue = null
            },
        ];

        public static readonly IFieldResolver Resolver = Resolvers.Sync<object, object?>((_, fieldContext, context) =>
        {
            var assetId = fieldContext.GetArgument<DomainId>("id");

            return context.GetAsset(assetId,
                fieldContext.CacheDuration());
        });
    }

    public static class Query
    {
        public static readonly QueryArguments Arguments =
        [
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
            },
        ];

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
        public static readonly QueryArguments Arguments =
        [
            new QueryArgument(Scalars.EnrichedAssetEventType)
            {
                Name = "type",
                Description = FieldDescriptions.EventType,
                DefaultValue = null
            },
        ];

        public static readonly ISourceStreamResolver Resolver = new SourceStreamResolver<object>(async fieldContext =>
        {
            var context = (GraphQLExecutionContext)fieldContext.UserContext;

            var app = context.Context.App;

            if (!context.Context.UserPermissions.Includes(PermissionIds.ForApp(PermissionIds.AppAssetsRead, app.Name)))
            {
                throw new DomainForbiddenException(T.Get("common.errorNoPermission"));
            }

            var key = $"asset-{app.Id}";

            var subscription = new AssetSubscription
            {
                Type = fieldContext.GetArgument<EnrichedAssetEventType?>("type")
            };

            var observable =
                await context.Resolve<ISubscriptionService>()
                    .SubscribeAsync(key, subscription, fieldContext.CancellationToken);

            return observable.OfType<EnrichedAssetEvent>();
        });
    }
}
