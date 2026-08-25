// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Claims;
using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Runtime;
using Jint.Runtime.Interop;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Assets;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Core.Scripting.Internal;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Domain.Apps.Entities.Properties;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities.Assets;

public sealed class AssetsJintExtension(IServiceProvider serviceProvider) : IJintExtension, IScriptDescriptor
{
    private delegate void UpdateAssetDelegate(JsValue asset, JsValue metadata);
    private delegate void GetAssetsDelegate(JsValue references, Action<JsValue> callback);
    private delegate void GetAssetTextDelegate(JsValue asset, Action<JsValue> callback, JsValue? encoding);
    private delegate void GetBlurHashDelegate(JsValue asset, Action<JsValue> callback, JsValue? componentX, JsValue? componentY);

    public void ExtendAsync(Engine engine)
    {
        AddGetAssetText(engine);
        AddGetAssetBlurHash(engine);
        AddGetAssetObject(engine);
        AddUpdateAsset(engine);
    }

    private void AddUpdateAsset(Engine engine)
    {
        var updateAsset = new UpdateAssetDelegate((asset, metadata) =>
        {
            if (!engine.TryGetVar<ClaimsPrincipal>("user", out var user))
            {
                throw new JavaScriptException("'updateAsset' is not available in this script.");
            }

            UpdateAsset(engine, user, asset, metadata);
        });

        engine.SetValue("updateAsset", updateAsset);
    }

    private void UpdateAsset(Engine engine, ClaimsPrincipal user, JsValue input, JsValue metadata)
    {
        // The javascript values are read while we are still inside the engine.
        if (!TryGetAssetRef(engine, input, out var asset) || metadata is not ObjectInstance metadataObj)
        {
            return;
        }

        var assetMetadata = new AssetMetadata();

        foreach (var (key, value) in metadataObj.GetOwnProperties())
        {
            assetMetadata[key.AsString()] = JsonMapper.Map(value.Value);
        }

        engine.Schedule(async ct =>
        {
            var commandBus = serviceProvider.GetRequiredService<ICommandBus>();

            var command = new AnnotateAsset
            {
                FromRule = true,
                AppId = asset.AppId,
                Actor = RefToken.Client("Script"),
                AssetId = asset.Id,
                Metadata = assetMetadata,
                User = user,
            };

            await commandBus.PublishAsync(command, default);
        });
    }

    private void AddGetAssetObject(Engine engine)
    {
        var getAssets = new GetAssetsDelegate((references, callback) =>
        {
            if (!engine.TryGetVar<DomainId>("appId", out var appId) ||
                !engine.TryGetVar<ClaimsPrincipal>("user", out var user))
            {
                throw new JavaScriptException("'getAssets' is not available in this script.");
            }

            GetAssets(engine, appId, user, references, callback);
        });

        var getAsset = new GetAssetsDelegate((references, callback) =>
        {
            if (!engine.TryGetVar<DomainId>("appId", out var appId) ||
                !engine.TryGetVar<ClaimsPrincipal>("user", out var user))
            {
                throw new JavaScriptException("'getAssetV2' is not available in this script.");
            }

            GetAsset(engine, appId, user, references, callback);
        });

        engine.SetValue("getAsset", getAssets);
        engine.SetValue("getAssetV2", getAsset);
        engine.SetValue("getAssets", getAssets);
    }

    private void AddGetAssetText(Engine engine)
    {
        var action = new GetAssetTextDelegate((references, callback, encoding) =>
        {
            GetText(engine, references, callback, encoding);
        });

        engine.SetValue("getAssetText", action);
    }

    private void AddGetAssetBlurHash(Engine engine)
    {
        var getBlurHash = new GetBlurHashDelegate((input, callback, componentX, componentY) =>
        {
            GetBlurHash(engine, input, callback, componentX, componentY);
        });

        engine.SetValue("getAssetBlurHash", getBlurHash);
    }

    private void GetText(Engine engine,
        JsValue input, Action<JsValue> callback, JsValue? encoding)
    {
        if (callback == null)
        {
            throw new JavaScriptException("Callback is not defined.");
        }

        // The javascript values are read while we are still inside the engine.
        TryGetAssetRef(engine, input, out var asset);

        var encodingName = encoding?.ToString();

        engine.Schedule(async ct =>
        {
            try
            {
                return await asset.GetTextAsync(encodingName, serviceProvider, ct);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                return null;
            }
        },
        text => callback(JsValue.FromObject(engine, text)));
    }

    private void GetBlurHash(Engine engine,
        JsValue input, Action<JsValue> callback, JsValue? componentX, JsValue? componentY)
    {
        if (callback == null)
        {
            throw new JavaScriptException("Callback is not defined.");
        }

        // The javascript values are read while we are still inside the engine.
        TryGetAssetRef(engine, input, out var asset);

        var options = new BlurOptions();

        if (componentX?.IsNumber() == true)
        {
            options.ComponentX = (int)componentX.AsNumber();
        }

        if (componentY?.IsNumber() == true)
        {
            options.ComponentX = (int)componentY.AsNumber();
        }

        engine.Schedule(async ct =>
        {
            try
            {
                return await asset.GetBlurHashAsync(options, serviceProvider, ct);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                return null;
            }
        },
        hash => callback(JsValue.FromObject(engine, hash)));
    }

    private void GetAssets(Engine engine, DomainId appId, ClaimsPrincipal user,
        JsValue references, Action<JsValue> callback)
    {
        if (callback == null)
        {
            throw new JavaScriptException("Callback is not defined.");
        }

        // The javascript values are read while we are still inside the engine.
        var ids = references.ToIds();

        if (ids.Count == 0)
        {
            callback(new JsArray(engine));
            return;
        }

        engine.Schedule(async ct =>
        {
            var app = await GetAppAsync(appId, ct);

            var assetQuery = serviceProvider.GetRequiredService<IAssetQueryService>();

            var requestContext =
                new Context(user, app).Clone(b => b
                    .WithNoTotal());

            return await assetQuery.QueryAsync(requestContext, null, Q.Empty.WithIds(ids), ct);
        },
        assets => callback(JsValue.FromObject(engine, assets.ToArray())));
    }

    private void GetAsset(Engine engine, DomainId appId, ClaimsPrincipal user,
        JsValue references, Action<JsValue> callback)
    {
        Guard.NotNull(callback);

        // The javascript values are read while we are still inside the engine.
        var ids = references.ToIds();

        if (ids.Count == 0)
        {
            callback(JsValue.Null);
            return;
        }

        engine.Schedule(async ct =>
        {
            var app = await GetAppAsync(appId, ct);

            var assetQuery = serviceProvider.GetRequiredService<IAssetQueryService>();

            var requestContext =
                new Context(user, app).Clone(b => b
                    .WithNoTotal());

            return await assetQuery.QueryAsync(requestContext, null, Q.Empty.WithIds(ids), ct);
        },
        assets => callback(JsValue.FromObject(engine, assets.FirstOrDefault())));
    }

    private static bool TryGetAssetRef(Engine engine, JsValue input, out AssetRef assetRef)
    {
        assetRef = default;

        if (input is not ObjectWrapper objectWrapper)
        {
            return false;
        }

        switch (objectWrapper.Target)
        {
            case Asset asset:
                assetRef = asset.ToRef();
                return true;

            case EnrichedAssetEvent @event:
                assetRef = @event.ToRef();
                return true;

            case AssetEntityScriptVars vars:
                if (!engine.TryGetVar<string>(nameof(AssetScriptVars.AppName), out var appName) ||
                    !engine.TryGetVar<DomainId>(nameof(AssetScriptVars.AppId), out var appId) ||
                    !engine.TryGetVar<DomainId>(nameof(AssetScriptVars.AssetId), out var assetId))
                {
                    return false;
                }

                engine.TryGetVar<string?>(nameof(AssetScriptVars.FileId), out var fileId);

                assetRef = new AssetRef(
                    NamedId.Of(appId, appName),
                    assetId,
                    vars.GetValue<long>(nameof(AssetEntityScriptVars.FileVersion)),
                    vars.GetValue<long>(nameof(AssetEntityScriptVars.FileSize)),
                    vars.GetValue<string>(nameof(AssetEntityScriptVars.MimeType)),
                    fileId,
                    vars.GetValue<AssetType>(nameof(AssetEntityScriptVars.Type)));
                return true;
        }

        return true;
    }

    private async Task<App> GetAppAsync(DomainId appId,
        CancellationToken ct)
    {
        var appProvider = serviceProvider.GetRequiredService<IAppProvider>();

        var app = await appProvider.GetAppAsync(appId, false, ct);

        return app ?? throw new JavaScriptException("App does not exist.");
    }

    public void Describe(AddDescription describe, ScriptScope scope)
    {
        if (!scope.HasFlag(ScriptScope.Async))
        {
            return;
        }

        describe(JsonType.Function, "getAsset(ids, callback)",
            Resources.ScriptingGetAsset,
            deprecationReason: Resources.ScriptingGetAssetDeprecated);

        describe(JsonType.Function, "getAssetV2(ids, callback)",
            Resources.ScriptingGetAssetV2);

        describe(JsonType.Function, "getAssets(ids, callback)",
            Resources.ScriptingGetAssets);

        describe(JsonType.Function, "getAssetText(asset, callback, encoding?)",
            Resources.ScriptingGetAssetText);

        describe(JsonType.Function, "getAssetBlurHash(asset, callback, x?, y?)",
            Resources.ScriptingGetBlurHash);

        describe(JsonType.Function, "updateAsset(asset, metadata)",
            Resources.ScriptingUpdateAsset);
    }
}
