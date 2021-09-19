// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
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
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Entities.Assets
{
    public sealed class AssetsJintExtension : IJintExtension
    {
        private delegate void GetAssetsDelegate(JsValue references, Action<JsValue> callback);
        private delegate void GetAssetTextDelegate(JsValue references, Action<JsValue> callback, JsValue encoding);
        private readonly IServiceProvider serviceProvider;

        public AssetsJintExtension(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public void ExtendAsync(ExecutionContext context)
        {
            AddAssetText(context);
            AddAsset(context);
        }

        private void AddAsset(ExecutionContext context)
        {
            if (!context.TryGetValue<DomainId>("appId", out var appId))
            {
                return;
            }

            if (!context.TryGetValue<ClaimsPrincipal>("user", out var user))
            {
                return;
            }

            var action = new GetAssetsDelegate((references, callback) => GetAssets(context, appId, user, references, callback));

            context.Engine.SetValue("getAsset", action);
            context.Engine.SetValue("getAssets", action);
        }

        private void AddAssetText(ExecutionContext context)
        {
            var action = new GetAssetTextDelegate((references, callback, encoding) => GetText(context, references, callback, encoding));

            context.Engine.SetValue("getAssetText", action);
        }

        private void GetText(ExecutionContext context, JsValue input, Action<JsValue> callback, JsValue encoding)
        {
            GetTextAsync(context, input, callback, encoding).Forget();
        }

        private async Task GetTextAsync(ExecutionContext context, JsValue input, Action<JsValue> callback, JsValue encoding)
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

                    var encoded = await assetFileStore.GetTextAsync(appId, id, fileVersion, encoding?.ToString());

                    callback(JsValue.FromObject(context.Engine, encoded));
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
                var assetItems = await assetQuery.QueryAsync(requestContext, null, Q.Empty.WithIds(ids), context.CancellationToken);

                callback(JsValue.FromObject(context.Engine, assetItems.ToArray()));
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
