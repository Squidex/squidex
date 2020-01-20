// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Contents.Queries;
using Squidex.Domain.Apps.Entities.Contents.Text;
using Squidex.Domain.Apps.Entities.History;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Orleans;

namespace Squidex.Config.Domain
{
    public static class ContentsServices
    {
        public static void AddSquidexContents(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<ContentOptions>(
                config.GetSection("contents"));

            services.AddSingletonAs(c => new Lazy<IContentQueryService>(c.GetRequiredService<IContentQueryService>))
                .AsSelf();

            services.AddSingletonAs<ContentQueryParser>()
                .AsSelf();

            services.AddTransientAs<ContentDomainObject>()
                .AsSelf();

            services.AddSingletonAs<ContentHistoryEventsCreator>()
                .As<IHistoryEventsCreator>();

            services.AddSingletonAs<ContentQueryService>()
                .As<IContentQueryService>();

            services.AddSingletonAs<ContentEnricher>()
                .As<IContentEnricher>();

            services.AddSingletonAs<ContentLoader>()
                .As<IContentLoader>();

            services.AddSingletonAs<DynamicContentWorkflow>()
                .AsOptional<IContentWorkflow>();

            services.AddSingletonAs<DefaultWorkflowsValidator>()
                .AsOptional<IWorkflowsValidator>();

            services.AddSingletonAs<GrainTextIndexer>()
                .As<ITextIndexer>().As<IEventConsumer>();

            services.AddSingletonAs<IndexManager>()
                .AsSelf();

            services.AddSingletonAs<GrainBootstrap<IContentSchedulerGrain>>()
                .AsSelf();
        }
    }
}