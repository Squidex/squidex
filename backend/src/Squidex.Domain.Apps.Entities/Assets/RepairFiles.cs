// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.IO;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Assets;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Entities.Assets
{
    public sealed class RepairFiles : IEventConsumer
    {
        private readonly IAssetFileStore assetFileStore;

        public string Name
        {
            get { return GetType().Name; }
        }

        public string EventsFilter
        {
            get { return "^asset\\-"; }
        }

        public RepairFiles(IAssetFileStore assetFileStore)
        {
            Guard.NotNull(assetFileStore, nameof(assetFileStore));

            this.assetFileStore = assetFileStore;
        }

        public async Task On(Envelope<IEvent> @event)
        {
            switch (@event.Payload)
            {
                case AssetCreated assetCreated:
                    await TryRepairAsync(assetCreated.AppId, assetCreated.AssetId, assetCreated.FileVersion);
                    break;
                case AssetUpdated assetUpdated:
                    await TryRepairAsync(assetUpdated.AppId, assetUpdated.AssetId, assetUpdated.FileVersion);
                    break;
            }
        }

        private async Task TryRepairAsync(NamedId<DomainId> appId, DomainId id, long fileVersion)
        {
            try
            {
                await assetFileStore.GetFileSizeAsync(appId.Id, id, fileVersion);
            }
            catch (AssetNotFoundException)
            {
                var stream = new MemoryStream(new byte[] { 1, 2, 3, 4 });

                await assetFileStore.UploadAsync(appId.Id, id, fileVersion, stream);
            }
        }
    }
}
