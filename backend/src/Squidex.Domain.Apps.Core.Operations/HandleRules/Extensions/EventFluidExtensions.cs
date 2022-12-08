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
        options.Filters.AddFilter("assetContentAppUrl", AssetContentAppUrl);
        options.Filters.AddFilter("assetContentSlugUrl", AssetContentSlugUrl);
    }

    private ValueTask<FluidValue> ContentUrl(FluidValue input, FilterArguments arguments, TemplateContext context)
    {
        var value = input.ToObjectValue();

        switch (value)
        {
            case DomainId id:
                {
                    if (context.GetValue("event")?.ToObjectValue() is EnrichedContentEvent contentEvent)
                    {
                        var result = urlGenerator.ContentUI(contentEvent.AppId, contentEvent.SchemaId, id);

                        return new StringValue(result);
                    }

                    break;
                }

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
        var value = input.ToObjectValue();

        switch (value)
        {
            case DomainId id:
                {
                    if (context.GetValue("event")?.ToObjectValue() is EnrichedAssetEvent assetEvent)
                    {
                        var result = urlGenerator.AssetContent(assetEvent.AppId, id.ToString());

                        return new StringValue(result);
                    }

                    break;
                }

            case EnrichedAssetEvent assetEvent:
                {
                    var result = urlGenerator.AssetContent(assetEvent.AppId, assetEvent.Id.ToString());

                    return new StringValue(result);
                }
        }

        return NilValue.Empty;
    }

    private ValueTask<FluidValue> AssetContentAppUrl(FluidValue input, FilterArguments arguments, TemplateContext context)
    {
        var value = input.ToObjectValue();

        switch (value)
        {
            case DomainId id:
                {
                    if (context.GetValue("event")?.ToObjectValue() is EnrichedAssetEvent assetEvent)
                    {
                        var result = urlGenerator.AssetContent(assetEvent.AppId, id.ToString());

                        return new StringValue(result);
                    }

                    break;
                }

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
        var value = input.ToObjectValue();

        switch (value)
        {
            case string s:
                {
                    if (context.GetValue("event")?.ToObjectValue() is EnrichedAssetEvent assetEvent)
                    {
                        var result = urlGenerator.AssetContent(assetEvent.AppId, s.Slugify());

                        return new StringValue(result);
                    }

                    break;
                }

            case EnrichedAssetEvent assetEvent:
                {
                    var result = urlGenerator.AssetContent(assetEvent.AppId, assetEvent.FileName.Slugify());

                    return new StringValue(result);
                }
        }

        return NilValue.Empty;
    }
}
