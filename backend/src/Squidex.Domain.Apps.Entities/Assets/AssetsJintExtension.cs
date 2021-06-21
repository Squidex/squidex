// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Jint.Native;
using Jint.Runtime;
using Jint.Runtime.Interop;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Infrastructure;
using Squidex.Infrastructure.ObjectPool;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Entities.Assets
{
    public sealed class AssetsJintExtension : IJintExtension
    {
        private delegate void GetAssetsDelegate(JsValue references, Action<JsValue> callback);
        private readonly IServiceProvider serviceProvider;

        public AssetsJintExtension(IServiceProvider serviceProvider)
        {
            Guard.NotNull(serviceProvider, nameof(serviceProvider));

            this.serviceProvider = serviceProvider;
        }

        public void ExtendAsync(ExecutionContext context)
        {
            AddAssetText(context);
            AddAsset(context);
        }

        private void AddAsset(ExecutionContext context)
        {
            if (!context.TryGetValue<DomainId>(nameof(ScriptVars.AppId), out var appId))
            {
                return;
            }

            if (!context.TryGetValue<ClaimsPrincipal>(nameof(ScriptVars.User), out var user))
            {
                return;
            }

            var action = new GetAssetsDelegate((references, callback) => GetAssets(context, appId, user, references, callback));

            context.Engine.SetValue("getAsset", action);
            context.Engine.SetValue("getAssets", action);
        }

        private void AddAssetText(ExecutionContext context)
        {
            var action = new GetAssetsDelegate((references, callback) => GetAssetText(context, references, callback));

            context.Engine.SetValue("getAssetText", action);
        }

        private void GetAssetText(ExecutionContext context, JsValue input, Action<JsValue> callback)
        {
            GetAssetTextCore(context, input, callback).Forget();
        }

        private async Task GetAssetTextCore(ExecutionContext context, JsValue input, Action<JsValue> callback)
        {
            Guard.NotNull(callback, nameof(callback));

            if (input is not ObjectWrapper objectWrapper)
            {
                callback(JsValue.FromObject(context.Engine, "ErrorNoAsset"));
                return;
            }

            async Task ResolveAssetText(DomainId appId, DomainId id, long fileSize, long fileVersion)
            {
                if (fileSize > 256_000)
                {
                    callback(JsValue.FromObject(context.Engine, "ErrorTooBig"));
                    return;
                }

                context.MarkAsync();

                try
                {
                    var assetFileStore = serviceProvider.GetRequiredService<IAssetFileStore>();

                    var tempStream = DefaultPools.MemoryStream.Get();
                    try
                    {
                        await assetFileStore!.DownloadAsync(appId, id, fileVersion, tempStream, default, context.CancellationToken);

                        tempStream.Position = 0;

                        using (var reader = new StreamReader(tempStream, leaveOpen: true))
                        {
                            var text = reader.ReadToEnd();

                            callback(JsValue.FromObject(context.Engine, text));
                        }
                    }
                    finally
                    {
                        DefaultPools.MemoryStream.Return(tempStream);
                    }
                }
                catch (Exception ex)
                {
                    context.Fail(ex);
                }
            }

            switch (objectWrapper.Target)
            {
                case IAssetEntity asset:
                    await ResolveAssetText(asset.AppId.Id, asset.Id, asset.FileSize, asset.FileVersion);
                    return;

                case EnrichedAssetEvent @event:
                    await ResolveAssetText(@event.AppId.Id, @event.Id, @event.FileSize, @event.FileVersion);
                    return;
            }

            callback(JsValue.FromObject(context.Engine, "ErrorNoAsset"));
        }

        private void GetAssets(ExecutionContext context, DomainId appId, ClaimsPrincipal user, JsValue references, Action<JsValue> callback)
        {
            GetReferencesAsync(context, appId, user, references, callback).Forget();
        }

        private async Task GetReferencesAsync(ExecutionContext context, DomainId appId, ClaimsPrincipal user, JsValue references, Action<JsValue> callback)
        {
            Guard.NotNull(callback, nameof(callback));

            var ids = new List<DomainId>();

            if (references.IsString())
            {
                ids.Add(DomainId.Create(references.ToString()));
            }
            else if (references.IsArray())
            {
                foreach (var value in references.AsArray())
                {
                    if (value.IsString())
                    {
                        ids.Add(DomainId.Create(value.ToString()));
                    }
                }
            }

            if (ids.Count == 0)
            {
                var emptyAssets = Array.Empty<IEnrichedAssetEntity>();

                callback(JsValue.FromObject(context.Engine, emptyAssets));
                return;
            }

            context.MarkAsync();

            try
            {
                var app = await GetAppAsync(appId);

                var requestContext =
                    new Context(user, app).Clone(b => b
                        .WithoutTotal());

                var assetQuery = serviceProvider.GetRequiredService<IAssetQueryService>();

                var assets = await assetQuery.QueryAsync(requestContext, null, Q.Empty.WithIds(ids), context.CancellationToken);

                callback(JsValue.FromObject(context.Engine, assets.ToArray()));
            }
            catch (Exception ex)
            {
                context.Fail(ex);
            }
        }

        private async Task<IAppEntity> GetAppAsync(DomainId appId)
        {
            var appProvider = serviceProvider.GetRequiredService<IAppProvider>();

            var app = await appProvider.GetAppAsync(appId);

            if (app == null)
            {
                throw new JavaScriptException("App does not exist.");
            }

            return app;
        }
    }
}
