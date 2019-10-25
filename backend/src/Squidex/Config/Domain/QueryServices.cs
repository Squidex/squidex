// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL;
using GraphQL.DataLoader;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Squidex.Domain.Apps.Core.ConvertContent;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Contents.GraphQL;
using Squidex.Infrastructure.Assets;
using Squidex.Web;
using Squidex.Web.Services;

namespace Squidex.Config.Domain
{
    public static class QueryServices
    {
        public static void AddSquidexQueries(this IServiceCollection services, IConfiguration config)
        {
            var exposeSourceUrl = config.GetOptionalValue("assetStore:exposeSourceUrl", true);

            services.AddSingletonAs(c => new UrlGenerator(
                    c.GetRequiredService<IOptions<UrlsOptions>>(),
                    c.GetRequiredService<IAssetStore>(),
                    exposeSourceUrl))
                .As<IGraphQLUrlGenerator>().As<IRuleUrlGenerator>().As<IAssetUrlGenerator>().As<IEmailUrlGenerator>();

            services.AddSingletonAs(x => new FuncDependencyResolver(t => x.GetRequiredService(t)))
                .As<IDependencyResolver>();

            services.AddSingletonAs<DataLoaderContextAccessor>()
                .As<IDataLoaderContextAccessor>();

            services.AddSingletonAs<DataLoaderDocumentListener>()
                .AsSelf();

            services.AddSingletonAs<CachingGraphQLService>()
                .As<IGraphQLService>();

            services.AddSingletonAs<CachingGraphQLService>()
                .As<IGraphQLService>();
        }
    }
}