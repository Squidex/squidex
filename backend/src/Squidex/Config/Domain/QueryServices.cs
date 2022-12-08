// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.ExtractReferenceIds;
using Squidex.Domain.Apps.Entities.Contents.GraphQL;
using Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Primitives;
using Squidex.Web.Services;

namespace Squidex.Config.Domain;

public static class QueryServices
{
    public static void AddSquidexQueries(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<GraphQLOptions>(config,
            "graphql");

        services.AddSingletonAs<StringReferenceExtractor>()
            .AsSelf();

        services.AddSingletonAs<UrlGenerator>()
            .As<IUrlGenerator>();

        services.AddSingletonAs<InstantGraphType>()
            .AsSelf();

        services.AddSingletonAs<JsonGraphType>()
            .AsSelf();

        services.AddSingletonAs<JsonNoopGraphType>()
            .AsSelf();
    }
}
