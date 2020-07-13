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

namespace Squidex.Domain.Apps.Core.HandleRules.Extensions
{
    public sealed class EventFluidExtensions : IFluidExtension
    {
        private readonly IUrlGenerator urlGenerator;

        public EventFluidExtensions(IUrlGenerator urlGenerator)
        {
            Guard.NotNull(urlGenerator, nameof(urlGenerator));

            this.urlGenerator = urlGenerator;
        }

        public void RegisterGlobalTypes(IMemberAccessStrategy memberAccessStrategy)
        {
            TemplateContext.GlobalFilters.AddFilter("contentUrl", ContentUrl);
            TemplateContext.GlobalFilters.AddFilter("assetContentUrl", AssetContentUrl);
        }

        private FluidValue ContentUrl(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            if (input is ObjectValue objectValue)
            {
                if (context.GetValue("event")?.ToObjectValue() is EnrichedContentEvent contentEvent)
                {
                    if (objectValue.ToObjectValue() is DomainId id && id != DomainId.Empty)
                    {
                        var result = urlGenerator.ContentUI(contentEvent.AppId, contentEvent.SchemaId, id);

                        return new StringValue(result);
                    }
                }
            }

            return NilValue.Empty;
        }

        private FluidValue AssetContentUrl(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            if (input is ObjectValue objectValue)
            {
                if (context.GetValue("event")?.ToObjectValue() is EnrichedEvent @event)
                {
                    if (objectValue.ToObjectValue() is DomainId id && id != DomainId.Empty)
                    {
                        var result = urlGenerator.AssetContent(@event.AppId, id);

                        return new StringValue(result);
                    }
                }
            }

            return NilValue.Empty;
        }
    }
}
