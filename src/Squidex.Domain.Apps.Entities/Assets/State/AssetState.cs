// ==========================================================================
//  AssetState.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Newtonsoft.Json;
using Squidex.Domain.Apps.Core.ValidateContent;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Infrastructure.Dispatching;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Entities.Assets.State
{
    public class AssetState : DomainObjectState<AssetState>,
        IAssetEntity,
        IAssetInfo,
        IUpdateableEntityWithAppRef
    {
        [JsonProperty]
        public Guid AppId { get; set; }

        [JsonProperty]
        public string FileName { get; set; }

        [JsonProperty]
        public string MimeType { get; set; }

        [JsonProperty]
        public long FileVersion { get; set; }

        [JsonProperty]
        public long FileSize { get; set; }

        [JsonProperty]
        public long TotalSize { get; set; }

        [JsonProperty]
        public bool IsImage { get; set; }

        [JsonProperty]
        public int? PixelWidth { get; set; }

        [JsonProperty]
        public int? PixelHeight { get; set; }

        [JsonProperty]
        public bool IsDeleted { get; set; }

        Guid IAssetInfo.AssetId
        {
            get { return Id; }
        }

        protected void On(AssetCreated @event)
        {
            SimpleMapper.Map(@event, this);

            TotalSize += @event.FileSize;
        }

        protected void On(AssetUpdated @event)
        {
            SimpleMapper.Map(@event, this);

            TotalSize += @event.FileSize;
        }

        protected void On(AssetRenamed @event)
        {
            FileName = @event.FileName;
        }

        protected void On(AssetDeleted @event)
        {
            IsDeleted = true;
        }

        public AssetState Apply(Envelope<IEvent> @event)
        {
            var payload = (SquidexEvent)@event.Payload;

            return Clone().Update(payload, @event.Headers, r => r.DispatchAction(payload));
        }
    }
}
