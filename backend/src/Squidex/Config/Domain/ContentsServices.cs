// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.ValidateContent;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Apps.Templates;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Contents.Counter;
using Squidex.Domain.Apps.Entities.Contents.Queries;
using Squidex.Domain.Apps.Entities.Contents.Queries.Steps;
using Squidex.Domain.Apps.Entities.Contents.Text;
using Squidex.Domain.Apps.Entities.Contents.Validation;
using Squidex.Domain.Apps.Entities.History;
using Squidex.Domain.Apps.Entities.Search;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Config.Domain;

public static class ContentsServices
{
    public static void AddSquidexContents(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<ContentOptions>(config,
            "contents");

        services.Configure<TemplatesOptions>(config,
            "templates");

        services.AddSingletonAs(c => new Lazy<IContentQueryService>(c.GetRequiredService<IContentQueryService>))
            .AsSelf();

        services.AddSingletonAs<ContentQueryParser>()
            .AsSelf();

        services.AddTransientAs<CounterService>()
            .As<ICounterService>().As<IDeleter>();

        services.AddTransientAs<ContentCache>()
            .As<IContentCache>();

        services.AddSingletonAs<DefaultValidatorsFactory>()
            .As<IValidatorsFactory>();

        services.AddSingletonAs<DependencyValidatorsFactory>()
            .As<IValidatorsFactory>();

        services.AddSingletonAs<ContentHistoryEventsCreator>()
            .As<IHistoryEventsCreator>();

        services.AddSingletonAs<ContentQueryService>()
            .As<IContentQueryService>();

        services.AddSingletonAs<ConvertData>()
            .As<IContentEnricherStep>();

        services.AddSingletonAs<CalculateTokens>()
            .As<IContentEnricherStep>();

        services.AddSingletonAs<EnrichForCaching>()
            .As<IContentEnricherStep>();

        services.AddSingletonAs<EnrichWithSchema>()
            .As<IContentEnricherStep>();

        services.AddSingletonAs<EnrichWithWorkflows>()
            .As<IContentEnricherStep>();

        services.AddSingletonAs<ResolveAssets>()
            .As<IContentEnricherStep>();

        services.AddSingletonAs<ResolveReferences>()
            .As<IContentEnricherStep>();

        services.AddSingletonAs<ScriptContent>()
            .As<IContentEnricherStep>();

        services.AddSingletonAs<ContentEnricher>()
            .As<IContentEnricher>();

        services.AddSingletonAs<ContentLoader>()
            .As<IContentLoader>();

        services.AddSingletonAs<DynamicContentWorkflow>()
            .AsOptional<IContentWorkflow>();

        services.AddSingletonAs<DefaultWorkflowsValidator>()
            .AsOptional<IWorkflowsValidator>();

        services.AddSingletonAs<TextIndexingProcess>()
            .As<IEventConsumer>();

        services.AddSingletonAs<ContentsSearchSource>()
            .As<ISearchSource>();

        services.AddSingletonAs<TemplatesClient>()
            .AsSelf();
    }
}
