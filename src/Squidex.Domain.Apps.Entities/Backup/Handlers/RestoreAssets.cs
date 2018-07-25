// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Assets.State;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Assets;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.States;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Entities.Backup.Handlers
{
    public sealed class RestoreAssets : HandlerBase, IRestoreHandler
    {
        private readonly HashSet<Guid> assetIds = new HashSet<Guid>();
        private readonly IAssetStore assetStore;

        public string Name { get; } = "Assets";

        public RestoreAssets(IStore<Guid> store, IAssetStore assetStore)
            : base(store)
        {
            Guard.NotNull(assetStore, nameof(assetStore));

            this.assetStore = assetStore;
        }

        public async Task HandleAsync(Envelope<IEvent> @event, Stream attachment)
        {
            var assetVersion = 0L;
            var assetId = Guid.Empty;

            switch (@event.Payload)
            {
                case AssetCreated assetCreated:
                    assetId = assetCreated.AssetId;
                    assetVersion = assetCreated.FileVersion;
                    assetIds.Add(assetCreated.AssetId);
                    break;
                case AssetUpdated asetUpdated:
                    assetId = asetUpdated.AssetId;
                    assetVersion = asetUpdated.FileVersion;
                    break;
            }

            if (attachment != null)
            {
                await assetStore.UploadAsync(assetId.ToString(), assetVersion, null, attachment);
            }
        }

        public Task ProcessAsync()
        {
            return RebuildManyAsync(assetIds, id => RebuildAsync<AssetState, AssetGrain>(id, (e, s) => s.Apply(e)));
        }

        public Task CompleteAsync()
        {
            return TaskHelper.Done;
        }
    }
}
