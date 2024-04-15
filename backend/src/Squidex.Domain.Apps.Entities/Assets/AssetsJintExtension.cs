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

public sealed class AssetsJintExtension : IJintExtension, IScriptDescriptor
{
    private delegate void UpdateAssetDelegate(JsValue asset, JsValue metadata);
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
        AddGetAssetText(context);
        AddGetAssetBlurHash(context);
        AddGetAssetObject(context);
        AddUpdateAsset(context);
    }

    private void AddUpdateAsset(ScriptExecutionContext context)
    {
        if (!context.TryGetValueIfExists<ClaimsPrincipal>("user", out var user))
        {
            return;
        }

        var updateAsset = new UpdateAssetDelegate((asset, metadata) =>
        {
            UpdateAsset(context, user, asset, metadata);
        });

        context.Engine.SetValue("updateAsset", updateAsset);
    }

    private void UpdateAsset(ScriptExecutionContext context, ClaimsPrincipal user, JsValue input, JsValue metadata)
    {
        context.Schedule(async (scheduler, ct) =>
        {
            if (!TryGetAssetRef(context, input, out var asset) || metadata is not ObjectInstance metadataObj)
            {
                return;
            }

            var commandBus = serviceProvider.GetRequiredService<ICommandBus>();

            var assetMetadata = new AssetMetadata();

            foreach (var (key, value) in metadataObj.GetOwnProperties())
            {
                assetMetadata[key.AsString()] = JsonMapper.Map(value.Value);
            }

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

    private void AddGetAssetObject(ScriptExecutionContext context)
    {
        if (!context.TryGetValueIfExists<DomainId>("appId", out var appId))
        {
            return;
        }

        if (!context.TryGetValueIfExists<ClaimsPrincipal>("user", out var user))
        {
            return;
        }

        var getAssets = new GetAssetsDelegate((references, callback) =>
        {
            GetAssets(context, appId, user, references, callback);
        });

        var getAsset = new GetAssetsDelegate((references, callback) =>
        {
            GetAsset(context, appId, user, references, callback);
        });

        context.Engine.SetValue("getAsset", getAssets);
        context.Engine.SetValue("getAssetV2", getAsset);
        context.Engine.SetValue("getAssets", getAssets);
    }

    private void AddGetAssetText(ScriptExecutionContext context)
    {
        var action = new GetAssetTextDelegate((references, callback, encoding) =>
        {
            GetText(context, references, callback, encoding);
        });

        context.Engine.SetValue("getAssetText", action);
    }

    private void AddGetAssetBlurHash(ScriptExecutionContext context)
    {
        var getBlurHash = new GetBlurHashDelegate((input, callback, componentX, componentY) =>
        {
            GetBlurHash(context, input, callback, componentX, componentY);
        });

        context.Engine.SetValue("getAssetBlurHash", getBlurHash);
    }

    private void GetText(ScriptExecutionContext context, JsValue input, Action<JsValue> callback, JsValue? encoding)
    {
        if (callback == null)
        {
            throw new JavaScriptException("Callback is not defined.");
        }

        context.Schedule(async (scheduler, ct) =>
        {
            TryGetAssetRef(context, input, out var asset);
            try
            {
                var text = await asset.GetTextAsync(encoding?.ToString(), serviceProvider, ct);

                scheduler.Run(callback, text);
            }
            catch
            {
                scheduler.Run(callback, JsValue.Null);
            }
        });
    }

    private void GetBlurHash(ScriptExecutionContext context, JsValue input, Action<JsValue> callback, JsValue? componentX, JsValue? componentY)
    {
        if (callback == null)
        {
            throw new JavaScriptException("Callback is not defined.");
        }

        context.Schedule(async (scheduler, ct) =>
        {
            TryGetAssetRef(context, input, out var asset);

            var options = new BlurOptions();

            if (componentX?.IsNumber() == true)
            {
                options.ComponentX = (int)componentX.AsNumber();
            }

            if (componentY?.IsNumber() == true)
            {
                options.ComponentX = (int)componentY.AsNumber();
            }

            try
            {
                var hash = await asset.GetBlurHashAsync(options, serviceProvider, ct);

                scheduler.Run(callback, hash);
            }
            catch
            {
                scheduler.Run(callback, JsValue.Null);
            }
        });
    }

    private void GetAssets(ScriptExecutionContext context, DomainId appId, ClaimsPrincipal user, JsValue references, Action<JsValue> callback)
    {
        if (callback == null)
        {
            throw new JavaScriptException("Callback is not defined.");
        }

        context.Schedule(async (scheduler, ct) =>
        {
            var ids = references.ToIds();

            if (ids.Count == 0)
            {
                scheduler.Run(callback, new JsArray(context.Engine));
                return;
            }

            var app = await GetAppAsync(appId, ct);

            if (app == null)
            {
                scheduler.Run(callback, new JsArray(context.Engine));
                return;
            }

            var assetQuery = serviceProvider.GetRequiredService<IAssetQueryService>();

            var requestContext =
                new Context(user, app).Clone(b => b
                    .WithNoTotal());

            var assets = await assetQuery.QueryAsync(requestContext, null, Q.Empty.WithIds(ids), ct);

            scheduler.Run(callback, JsValue.FromObject(context.Engine, assets.ToArray()));
            return;
        });
    }

    private void GetAsset(ScriptExecutionContext context, DomainId appId, ClaimsPrincipal user, JsValue references, Action<JsValue> callback)
    {
        Guard.NotNull(callback);

        context.Schedule(async (scheduler, ct) =>
        {
            var ids = references.ToIds();

            if (ids.Count == 0)
            {
                scheduler.Run(callback, JsValue.Null);
                return;
            }

            var app = await GetAppAsync(appId, ct);

            if (app == null)
            {
                scheduler.Run(callback, JsValue.Null);
                return;
            }

            var assetQuery = serviceProvider.GetRequiredService<IAssetQueryService>();

            var requestContext =
                new Context(user, app).Clone(b => b
                    .WithNoTotal());

            var assets = await assetQuery.QueryAsync(requestContext, null, Q.Empty.WithIds(ids), ct);

            scheduler.Run(callback, JsValue.FromObject(context.Engine, assets.FirstOrDefault()));
            return;
        });
    }

    private static bool TryGetAssetRef(ScriptExecutionContext context, JsValue input, out AssetRef assetRef)
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
                if (!context.TryGetValueIfExists<string>(nameof(AssetScriptVars.AppName), out var appName) ||
                    !context.TryGetValueIfExists<DomainId>(nameof(AssetScriptVars.AppId), out var appId) ||
                    !context.TryGetValueIfExists<DomainId>(nameof(AssetScriptVars.AssetId), out var assetId))
                {
                    return false;
                }

                context.TryGetValueIfExists<string?>(nameof(AssetScriptVars.FileId), out var fileId);

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
