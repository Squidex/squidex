// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Claims;
using Jint;
using Jint.Native;
using Jint.Runtime;
using Jint.Runtime.Interop;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Assets;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Properties;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Assets;

public sealed class AssetsJintExtension : IJintExtension, IScriptDescriptor
{
    private delegate void GetAssetsDelegate(JsValue references, Action<JsValue> callback);
    private delegate void GetAssetTextDelegate(JsValue asset, Action<JsValue> callback, JsValue? encoding);
    private delegate void GetBlurHashDelegate(JsValue asset, Action<JsValue> callback, JsValue? componentX, JsValue? componentY);
    private readonly IServiceProvider serviceProvider;

    public AssetsJintExtension(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    public void ExtendAsync(ScriptExecutionContext context)
    {
        AddAssetText(context);
        AddAssetBlurHash(context);
        AddAsset(context);
    }

    public void Describe(AddDescription describe, ScriptScope scope)
    {
        if (!scope.HasFlag(ScriptScope.Async))
        {
            return;
        }

        describe(JsonType.Function, "getAssets(ids, callback)",
            Resources.ScriptingGetAssets);

        describe(JsonType.Function, "getAsset(ids, callback)",
            Resources.ScriptingGetAsset);

        describe(JsonType.Function, "getAssetText(asset, callback, encoding?)",
            Resources.ScriptingGetAssetText);

        describe(JsonType.Function, "getAssetBlurHash(asset, callback, x?, y?)",
            Resources.ScriptingGetBlurHash);
    }

    private void AddAsset(ScriptExecutionContext context)
    {
        if (!context.TryGetValue<DomainId>("appId", out var appId))
        {
            return;
        }

        if (!context.TryGetValue<ClaimsPrincipal>("user", out var user))
        {
            return;
        }

        var getAssets = new GetAssetsDelegate((references, callback) =>
        {
            GetAssets(context, appId, user, references, callback);
        });

        context.Engine.SetValue("getAsset", getAssets);
        context.Engine.SetValue("getAssets", getAssets);
    }

    private void AddAssetText(ScriptExecutionContext context)
    {
        var action = new GetAssetTextDelegate((references, callback, encoding) =>
        {
            GetText(context, references, callback, encoding);
        });

        context.Engine.SetValue("getAssetText", action);
    }

    private void AddAssetBlurHash(ScriptExecutionContext context)
    {
        var getBlurHash = new GetBlurHashDelegate((input, callback, componentX, componentY) =>
        {
            GetBlurHash(context, input, callback, componentX, componentY);
        });

        context.Engine.SetValue("getAssetBlurHash", getBlurHash);
    }

    private void GetText(ScriptExecutionContext context, JsValue input, Action<JsValue> callback, JsValue? encoding)
    {
        Guard.NotNull(callback);

        context.Schedule(async (scheduler, ct) =>
        {
            if (input is not ObjectWrapper objectWrapper)
            {
                scheduler.Run(callback, JsValue.FromObject(context.Engine, "ErrorNoAsset"));
                return;
            }

            async Task ResolveAssetText(AssetRef asset)
            {
                if (asset.FileSize > 256_000)
                {
                    scheduler.Run(callback, JsValue.FromObject(context.Engine, "ErrorTooBig"));
                    return;
                }

                var assetFileStore = serviceProvider.GetRequiredService<IAssetFileStore>();
                try
                {
                    var text = await asset.GetTextAsync(encoding?.ToString(), assetFileStore, ct);

                    scheduler.Run(callback, JsValue.FromObject(context.Engine, text));
                }
                catch
                {
                    scheduler.Run(callback, JsValue.Null);
                }
            }

            switch (objectWrapper.Target)
            {
                case IAssetEntity asset:
                    await ResolveAssetText(asset.ToRef());
                    break;

                case EnrichedAssetEvent e:
                    await ResolveAssetText(e.ToRef());
                    break;

                default:
                    scheduler.Run(callback, JsValue.FromObject(context.Engine, "ErrorNoAsset"));
                    break;
            }
        });
    }

    private void GetBlurHash(ScriptExecutionContext context, JsValue input, Action<JsValue> callback, JsValue? componentX, JsValue? componentY)
    {
        Guard.NotNull(callback);

        context.Schedule(async (scheduler, ct) =>
        {
            if (input is not ObjectWrapper objectWrapper)
            {
                scheduler.Run(callback, JsValue.FromObject(context.Engine, "ErrorNoAsset"));
                return;
            }

            async Task ResolveHashAsync(AssetRef asset)
            {
                if (asset.FileSize > 512_000 || asset.Type != AssetType.Image)
                {
                    scheduler.Run(callback, JsValue.Null);
                    return;
                }

                var options = new BlurOptions();

                if (componentX?.IsNumber() == true)
                {
                    options.ComponentX = (int)componentX.AsNumber();
                }

                if (componentY?.IsNumber() == true)
                {
                    options.ComponentX = (int)componentX.AsNumber();
                }

                var assetThumbnailGenerator = serviceProvider.GetRequiredService<IAssetThumbnailGenerator>();
                var assetFileStore = serviceProvider.GetRequiredService<IAssetFileStore>();
                try
                {
                    var hash = await asset.GetBlurHashAsync(options, assetFileStore, assetThumbnailGenerator, ct);

                    scheduler.Run(callback, JsValue.FromObject(context.Engine, hash));
                }
                catch
                {
                    scheduler.Run(callback, JsValue.Null);
                }
            }

            switch (objectWrapper.Target)
            {
                case IAssetEntity asset:
                    await ResolveHashAsync(asset.ToRef());
                    break;

                case EnrichedAssetEvent @event:
                    await ResolveHashAsync(@event.ToRef());
                    break;

                default:
                    scheduler.Run(callback, JsValue.FromObject(context.Engine, "ErrorNoAsset"));
                    break;
            }
        });
    }

    private void GetAssets(ScriptExecutionContext context, DomainId appId, ClaimsPrincipal user, JsValue references, Action<JsValue> callback)
    {
        Guard.NotNull(callback);

        context.Schedule(async (scheduler, ct) =>
        {
            var ids = references.ToIds();

            if (ids.Count == 0)
            {
                var emptyAssets = Array.Empty<IEnrichedAssetEntity>();

                scheduler.Run(callback, JsValue.FromObject(context.Engine, emptyAssets));
                return;
            }

            var app = await GetAppAsync(appId, ct);

            if (app == null)
            {
                var emptyAssets = Array.Empty<IEnrichedAssetEntity>();

                scheduler.Run(callback, JsValue.FromObject(context.Engine, emptyAssets));
                return;
            }

            var assetQuery = serviceProvider.GetRequiredService<IAssetQueryService>();

            var requestContext =
                new Context(user, app).Clone(b => b
                    .WithoutTotal());

            var assets = await assetQuery.QueryAsync(requestContext, null, Q.Empty.WithIds(ids), ct);

            scheduler.Run(callback, JsValue.FromObject(context.Engine, assets.ToArray()));
            return;
        });
    }

    private async Task<IAppEntity> GetAppAsync(DomainId appId,
        CancellationToken ct)
    {
        var appProvider = serviceProvider.GetRequiredService<IAppProvider>();

        var app = await appProvider.GetAppAsync(appId, false, ct);

        if (app == null)
        {
            throw new JavaScriptException("App does not exist.");
        }

        return app;
    }
}
