// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL;
using GraphQL.Server;
using GraphQL.Server.Transports.AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Squidex.Config.Domain;
using Squidex.Domain.Apps.Entities;
using Squidex.Infrastructure.Caching;
using Squidex.Pipeline.Plugins;
using Squidex.Web;
using Squidex.Web.GraphQL;
using Squidex.Web.Pipeline;
using Squidex.Web.Services;

namespace Squidex.Config.Web
{
    public static class WebServices
    {
        public static void AddSquidexMvcWithPlugins(this IServiceCollection services, IConfiguration config)
        {
            services.AddDefaultWebServices(config);
            services.AddDefaultForwardRules();

            services.AddSingletonAs(c => new ExposedValues(c.GetRequiredService<IOptions<ExposedConfiguration>>().Value, config, typeof(WebServices).Assembly))
                .AsSelf();

            services.AddSingletonAs<FileCallbackResultExecutor>()
                .AsSelf();

            services.AddSingletonAs<ApiCostsFilter>()
                .AsSelf();

            services.AddSingletonAs<AppResolver>()
                .AsSelf();

            services.AddSingletonAs<SchemaResolver>()
                .AsSelf();

            services.AddSingletonAs<UsageMiddleware>()
                .AsSelf();

            services.AddSingletonAs<StringLocalizer>()
                .As<IStringLocalizer>().As<IStringLocalizerFactory>();

            services.AddSingletonAs<CachingManager>()
                .As<IRequestCache>();

            services.AddSingletonAs<ContextProvider>()
                .As<IContextProvider>();

            services.AddSingletonAs<HttpContextAccessor>()
                .As<IHttpContextAccessor>();

            services.AddSingletonAs<ActionContextAccessor>()
                .As<IActionContextAccessor>();

            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressInferBindingSourcesForParameters = true;
                options.SuppressModelStateInvalidFilter = true;
            });

            services.AddLocalization();

            services.AddMvc(options =>
            {
                options.Filters.Add<CachingFilter>();
                options.Filters.Add<DeferredActionFilter>();
                options.Filters.Add<AppResolver>();
                options.Filters.Add<SchemaResolver>();
                options.Filters.Add<MeasureResultFilter>();
            })
            .AddDataAnnotationsLocalization()
            .AddRazorRuntimeCompilation()
            .AddSquidexPlugins(config)
            .AddSquidexSerializers();
        }

        public static void AddSquidexGraphQL(this IServiceCollection services)
        {
            services.AddGraphQL(options =>
            {
                options.EnableMetrics = false;
            })
            .AddDataLoader()
            .AddSystemTextJson()
            .AddSquidexWriter();

            services.AddSingletonAs<DummySchema>()
                .AsSelf();

            services.AddSingletonAs<DynamicExecutor>()
                .As<IDocumentExecuter>();

            services.AddSingletonAs<DynamicUserContextBuilder>()
                .As<IUserContextBuilder>();

            services.AddSingletonAs<GraphQLMiddleware>()
                .AsSelf();
        }
    }
}
