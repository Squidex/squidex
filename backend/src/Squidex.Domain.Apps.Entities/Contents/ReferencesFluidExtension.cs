// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.Encodings.Web;
using Fluid;
using Fluid.Ast;
using Fluid.Tags;
using Fluid.Values;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Core.Templates;
using Squidex.Infrastructure;

#pragma warning disable CA1826 // Do not use Enumerable methods on indexable collections

namespace Squidex.Domain.Apps.Entities.Contents;

public sealed class ReferencesFluidExtension : IFluidExtension
{
    private static readonly FluidValue ErrorNullReference = FluidValue.Create(null);
    private readonly IServiceProvider serviceProvider;

    private sealed class ReferenceTag : ArgumentsTag
    {
        private readonly IServiceProvider serviceProvider;

        public ReferenceTag(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public override async ValueTask<Completion> WriteToAsync(TextWriter writer,
            TextEncoder encoder, TemplateContext context, FilterArgument[] arguments)
        {
            if (arguments.Length == 2 && context.GetValue("event")?.ToObjectValue() is EnrichedEvent enrichedEvent)
            {
                var id = await arguments[1].Expression.EvaluateAsync(context);

                var content = await ResolveContentAsync(serviceProvider, enrichedEvent.AppId.Id, id);

                if (content != null)
                {
                    var name = (await arguments[0].Expression.EvaluateAsync(context)).ToStringValue();

                    context.SetValue(name, content);
                }
            }

            return Completion.Normal;
        }
    }

    public ReferencesFluidExtension(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    public void RegisterGlobalTypes(IMemberAccessStrategy memberAccessStrategy)
    {
        memberAccessStrategy.Register<IContentEntity>();
        memberAccessStrategy.Register<IWithId<DomainId>>();
        memberAccessStrategy.Register<IEntity>();
        memberAccessStrategy.Register<IEntityWithCreatedBy>();
        memberAccessStrategy.Register<IEntityWithLastModifiedBy>();
        memberAccessStrategy.Register<IEntityWithVersion>();
        memberAccessStrategy.Register<IEnrichedContentEntity>();

        AddReferenceFilter();
    }

    private void AddReferenceFilter()
    {
        TemplateContext.GlobalFilters.AddAsyncFilter("reference", async (input, arguments, context) =>
        {
            if (context.GetValue("event")?.ToObjectValue() is EnrichedEvent enrichedEvent)
            {
                var content = await ResolveContentAsync(serviceProvider, enrichedEvent.AppId.Id, input);

                if (content == null)
                {
                    return ErrorNullReference;
                }

                return FluidValue.Create(content);
            }

            return ErrorNullReference;
        });
    }

    public void RegisterLanguageExtensions(FluidParserFactory factory)
    {
        factory.RegisterTag("reference", new ReferenceTag(serviceProvider));
    }

    private static async Task<IContentEntity?> ResolveContentAsync(IServiceProvider serviceProvider, DomainId appId, FluidValue id)
    {
        var appProvider = serviceProvider.GetRequiredService<IAppProvider>();

        var app = await appProvider.GetAppAsync(appId);

        if (app == null)
        {
            return null;
        }

        var domainId = DomainId.Create(id.ToStringValue());

        var contentQuery = serviceProvider.GetRequiredService<IContentQueryService>();

        var requestContext =
            Context.Admin(app).Clone(b => b
                .WithoutContentEnrichment()
                .WithUnpublished()
                .WithoutTotal());

        var contents = await contentQuery.QueryAsync(requestContext, Q.Empty.WithIds(domainId));

        return contents.FirstOrDefault();
    }
}
