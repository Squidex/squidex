// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.Encodings.Web;
using Fluid;
using Fluid.Ast;
using Fluid.Values;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Core.Templates;
using Squidex.Infrastructure;
using static Parlot.Fluent.Parsers;

#pragma warning disable CA1826 // Do not use Enumerable methods on indexable collections

namespace Squidex.Domain.Apps.Entities.Contents;

public sealed class ReferencesFluidExtension : IFluidExtension
{
    private readonly IServiceProvider serviceProvider;

    public ReferencesFluidExtension(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    public void RegisterLanguageExtensions(CustomFluidParser parser, TemplateOptions options)
    {
        AddReferenceFilter(options);

        parser.RegisterParserTag("reference",
            parser.PrimaryParser.AndSkip(ZeroOrOne(parser.CommaParser)).And(parser.PrimaryParser),
            ResolveReference);
    }

    private async ValueTask<Completion> ResolveReference(ValueTuple<Expression, Expression> arguments, TextWriter writer, TextEncoder encoder, TemplateContext context)
    {
        if (context.GetValue("event")?.ToObjectValue() is not EnrichedEvent enrichedEvent)
        {
            return Completion.Normal;
        }

        var (nameArg, idArg) = arguments;

        var contentId = await idArg.EvaluateAsync(context);
        var content = await ResolveContentAsync(serviceProvider, enrichedEvent.AppId.Id, contentId);

        if (content != null)
        {
            var name = (await nameArg.EvaluateAsync(context)).ToStringValue();

            context.SetValue(name, content);
        }

        return Completion.Normal;
    }

    private void AddReferenceFilter(TemplateOptions options)
    {
        options.Filters.AddFilter("reference", async (input, arguments, context) =>
        {
            if (context.GetValue("event")?.ToObjectValue() is EnrichedEvent enrichedEvent)
            {
                var content = await ResolveContentAsync(serviceProvider, enrichedEvent.AppId.Id, input);

                if (content == null)
                {
                    return NilValue.Instance;
                }

                return FluidValue.Create(content, options);
            }

            return NilValue.Instance;
        });
    }

    private static async Task<Content?> ResolveContentAsync(IServiceProvider serviceProvider, DomainId appId, FluidValue id)
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
                .WithFields(null)
                .WithNoEnrichment()
                .WithUnpublished()
                .WithNoTotal());

        var contents = await contentQuery.QueryAsync(requestContext, Q.Empty.WithIds(domainId));

        return contents.FirstOrDefault();
    }
}
