// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Entities.Contents.GraphQL;
using Squidex.Domain.Apps.Entities.Contents.GraphQL.Types;
using Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Primitives;
using Squidex.Web.Services;

namespace Squidex.Config.Domain
{
    public static class QueryServices
    {
        public static void AddSquidexQueries(this IServiceCollection services, IConfiguration config)
        {
            var exposeSourceUrl = config.GetOptionalValue("assetStore:exposeSourceUrl", true);

            services.Configure<GraphQLOptions>(config,
                "graphql");

            services.AddSingletonAs(c => ActivatorUtilities.CreateInstance<UrlGenerator>(c, exposeSourceUrl))
                .As<IUrlGenerator>();

            services.AddSingletonAs<SharedTypes>()
                .AsSelf();

            services.AddSingletonAs<InstantGraphType>()
                .AsSelf();

            services.AddSingletonAs<JsonGraphType>()
                .AsSelf();

            services.AddSingletonAs<JsonNoopGraphType>()
                .AsSelf();

            services.AddSingletonAs<CachingGraphQLService>()
                .As<IGraphQLService>();
        }
    }
}