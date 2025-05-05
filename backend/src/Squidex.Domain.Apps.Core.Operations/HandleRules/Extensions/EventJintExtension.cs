// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

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

    private sealed class FlowConsoleWrapper
    {
        public static readonly FlowConsoleWrapper Instance = new FlowConsoleWrapper();

#pragma warning disable CA1822 // Mark members as static
        private void LogCore(string? message, string prefix)
#pragma warning restore CA1822 // Mark members as static
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(prefix))
            {
                message = $"{prefix}: {message}";
            }

            FlowConsole.Out(message);
        }

        public void Log(string message)
        {
            LogCore(message, string.Empty);
        }

        public void Info(string message)
        {
            LogCore(message, "INFO");
        }

        public void Warn(string message)
        {
            LogCore(message, "WARN");
        }

        public void Error(string message)
        {
            LogCore(message, "ERROR");
        }

        public void Debug(string message)
        {
            LogCore(message, "DEBUG");
        }
    }

    public void Extend(ScriptExecutionContext context)
    {
        context.Engine.SetValue("console", FlowConsoleWrapper.Instance);

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

        context.Engine.SetValue("assetContentSlugUrl", new EventDelegate(() =>
        {
            if (context.TryGetValue("event", out var temp) && temp is EnrichedAssetEvent assetEvent)
            {
                return urlGenerator.AssetContent(assetEvent.AppId, assetEvent.FileName.Slugify());
            }

            return JsValue.Null;
        }));

        var assetUrl = new EventDelegate(() =>
        {
            if (context.TryGetValue("event", out var temp) && temp is EnrichedAssetEvent assetEvent)
            {
                return urlGenerator.AssetContent(assetEvent.AppId, assetEvent.Id.ToString());
            }

            return JsValue.Null;
        });

        context.Engine.SetValue("assetContentUrl", assetUrl);
        context.Engine.SetValue("assetContentAppUrl", assetUrl);
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
