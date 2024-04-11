// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Fluid;
using Fluid.Values;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Core.Templates;
using Squidex.Infrastructure;
using Squidex.Text;

namespace Squidex.Domain.Apps.Core.HandleRules.Extensions;

public sealed class EventFluidExtensions : IFluidExtension
{
    private readonly IUrlGenerator urlGenerator;

    public EventFluidExtensions(IUrlGenerator urlGenerator)
    {
        this.urlGenerator = urlGenerator;
    }

    public void RegisterLanguageExtensions(CustomFluidParser parser, TemplateOptions options)
    {
        options.Filters.AddFilter("contentUrl", ContentUrl);
        options.Filters.AddFilter("assetContentUrl", AssetContentUrl);
        options.Filters.AddFilter("assetContentAppUrl", AssetContentUrl);
        options.Filters.AddFilter("assetContentSlugUrl", AssetContentSlugUrl);
    }

    private ValueTask<FluidValue> ContentUrl(FluidValue input, FilterArguments arguments, TemplateContext context)
    {
        FluidValue TryResolveId(TemplateContext context, DomainId id)
        {
            if (context.GetValue("event")?.ToObjectValue() is EnrichedContentEvent contentEvent)
            {
                var url = urlGenerator.ContentUI(contentEvent.AppId, contentEvent.SchemaId, id);

                return new StringValue(url);
            }

            return NilValue.Empty;
        }

        var value = input.ToObjectValue();

        switch (value)
        {
            case DomainId id:
                return TryResolveId(context, id);

            case string id:
                return TryResolveId(context, DomainId.Create(id));

            case EnrichedContentEvent contentEvent:
                {
                    var result = urlGenerator.ContentUI(contentEvent.AppId, contentEvent.SchemaId, contentEvent.Id);

                    return new StringValue(result);
                }
        }

        return NilValue.Empty;
    }

    private ValueTask<FluidValue> AssetContentUrl(FluidValue input, FilterArguments arguments, TemplateContext context)
    {
        FluidValue TryResolveId(TemplateContext context, string id)
        {
            if (context.GetValue("event")?.ToObjectValue() is EnrichedEvent enrichedEvent)
            {
                var result = urlGenerator.AssetContent(enrichedEvent.AppId, id);

                return new StringValue(result);
            }

            return NilValue.Empty;
        }

        var value = input.ToObjectValue();

        switch (value)
        {
            case DomainId id:
                return TryResolveId(context, id.ToString());

            case string id:
                return TryResolveId(context, id);

            case EnrichedAssetEvent assetEvent:
                {
                    var result = urlGenerator.AssetContent(assetEvent.AppId, assetEvent.Id.ToString());

                    return new StringValue(result);
                }
        }

        return NilValue.Empty;
    }

    private ValueTask<FluidValue> AssetContentSlugUrl(FluidValue input, FilterArguments arguments, TemplateContext context)
    {
        FluidValue TryResolveSlug(TemplateContext context, string slug)
        {
            if (context.GetValue("event")?.ToObjectValue() is EnrichedEvent enrichedEvent)
            {
                var result = urlGenerator.AssetContent(enrichedEvent.AppId, slug.Slugify());

                return new StringValue(result);
            }

            return NilValue.Empty;
        }

        var value = input.ToObjectValue();

        switch (value)
        {
            case string s:
                return TryResolveSlug(context, s);

            case EnrichedAssetEvent assetEvent:
                {
                    var result = urlGenerator.AssetContent(assetEvent.AppId, assetEvent.FileName.Slugify());

                    return new StringValue(result);
                }
        }

        return NilValue.Empty;
    }
}
