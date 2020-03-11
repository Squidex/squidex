﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Jint.Native;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.HandleRules.Scripting
{
    public sealed class EventScriptExtension : IScriptExtension
    {
        private delegate JsValue EventDelegate();
        private readonly IUrlGenerator urlGenerator;

        public EventScriptExtension(IUrlGenerator urlGenerator)
        {
            Guard.NotNull(urlGenerator);

            this.urlGenerator = urlGenerator;
        }

        public void Extend(ExecutionContext context, bool async)
        {
            context.Engine.SetValue("contentAction", new EventDelegate(() =>
            {
                if (context.TryGetValue("event", out var temp) && temp is EnrichedContentEvent contentEvent)
                {
                    return contentEvent.Status.ToString();
                }

                return JsValue.Null;
            }));

            context.Engine.SetValue("contentUrl", new EventDelegate(() =>
            {
                if (context.TryGetValue("event", out var temp) && temp is EnrichedContentEvent contentEvent)
                {
                    return urlGenerator.ContentUI(contentEvent.AppId, contentEvent.SchemaId, contentEvent.Id);
                }

                return JsValue.Null;
            }));

            context.Engine.SetValue("assetContentUrl", new EventDelegate(() =>
            {
                if (context.TryGetValue("event", out var temp) && temp is EnrichedAssetEvent assetEvent)
                {
                    return urlGenerator.AssetContent(assetEvent.Id);
                }

                return JsValue.Null;
            }));
        }
    }
}
