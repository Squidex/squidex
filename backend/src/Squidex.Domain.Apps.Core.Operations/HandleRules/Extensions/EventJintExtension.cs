// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Jint;
using Jint.Native;
using Squidex.Domain.Apps.Core.Properties;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Flows;
using Squidex.Text;

namespace Squidex.Domain.Apps.Core.HandleRules.Extensions;

public sealed class EventJintExtension(IUrlGenerator urlGenerator) : IJintExtension, IScriptDescriptor
{
    private delegate JsValue EventDelegate();

    public void Extend(Engine engine)
    {
        engine.SetValue("contentAction", new EventDelegate(() =>
        {
            if (engine.TryGetVar<EnrichedContentEvent>("event", out var contentEvent))
            {
                return contentEvent.Status.ToString();
            }

            return JsValue.Null;
        }));

        engine.SetValue("contentUrl", new EventDelegate(() =>
        {
            if (engine.TryGetVar<EnrichedContentEvent>("event", out var contentEvent))
            {
                return urlGenerator.ContentUI(contentEvent.AppId, contentEvent.SchemaId, contentEvent.Id);
            }

            return JsValue.Null;
        }));

        engine.SetValue("assetContentSlugUrl", new EventDelegate(() =>
        {
            if (engine.TryGetVar<EnrichedAssetEvent>("event", out var assetEvent))
            {
                return urlGenerator.AssetContent(assetEvent.AppId, assetEvent.FileName.Slugify());
            }

            return JsValue.Null;
        }));

        var assetUrl = new EventDelegate(() =>
        {
            if (engine.TryGetVar<EnrichedAssetEvent>("event", out var assetEvent))
            {
                return urlGenerator.AssetContent(assetEvent.AppId, assetEvent.Id.ToString());
            }

            return JsValue.Null;
        });

        engine.SetValue("assetContentUrl", assetUrl);
        engine.SetValue("assetContentAppUrl", assetUrl);
    }

    public void Describe(AddDescription describe, ScriptScope scope)
    {
        if (scope.HasFlag(ScriptScope.ContentTrigger))
        {
            describe(JsonType.Function, "contentAction",
                Resources.ScriptingContentAction);

            describe(JsonType.Function, "contentUrl",
                Resources.ScriptingContentUrl);
        }

        if (scope.HasFlag(ScriptScope.AssetTrigger))
        {
            describe(JsonType.Function, "assetContentUrl",
                Resources.ScriptingAssetContentUrl);

            describe(JsonType.Function, "assetContentAppUrl",
                Resources.ScriptingAssetContentAppUrl);

            describe(JsonType.Function, "assetContentSlugUrl",
                Resources.ScriptingAssetContentSlugUrl);
        }
    }
}
