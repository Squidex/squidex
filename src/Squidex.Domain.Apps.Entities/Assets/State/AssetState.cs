// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Squidex.Domain.Apps.Core.ValidateContent;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Dispatching;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Entities.Assets.State
{
    public class AssetState : DomainObjectState<AssetState>, IAssetEntity
    {
        [DataMember]
        public NamedId<Guid> AppId { get; set; }

        [DataMember]
        public string FileName { get; set; }

        [DataMember]
        public string MimeType { get; set; }

        [DataMember]
        public string Slug { get; set; }

        [DataMember]
        public long FileVersion { get; set; }

        [DataMember]
        public long FileSize { get; set; }

        [DataMember]
        public long TotalSize { get; set; }

        [DataMember]
        public bool IsImage { get; set; }

        [DataMember]
        public int? PixelWidth { get; set; }

        [DataMember]
        public int? PixelHeight { get; set; }

        [DataMember]
        public bool IsDeleted { get; set; }

        [DataMember]
        public HashSet<string> Tags { get; set; }

        Guid IAssetInfo.AssetId
        {
            get { return Id; }
        }

        protected void On(AssetCreated @event)
        {
            SimpleMapper.Map(@event, this);

            FileName = @event.FileName;

            if (string.IsNullOrWhiteSpace(@event.Slug))
            {
                Slug = @event.FileName.ToAssetSlug();
            }
            else
            {
                Slug = @event.Slug;
            }

            TotalSize += @event.FileSize;
        }

        protected void On(AssetUpdated @event)
        {
            SimpleMapper.Map(@event, this);

            TotalSize += @event.FileSize;
        }

        protected void On(AssetAnnotated @event)
        {
            if (!string.IsNullOrWhiteSpace(@event.FileName))
            {
                FileName = @event.FileName;
            }

            if (!string.IsNullOrWhiteSpace(@event.Slug))
            {
                Slug = @event.Slug;
            }

            if (@event.Tags != null)
            {
                Tags = @event.Tags;
            }
        }

        protected void On(AssetDeleted @event)
        {
            IsDeleted = true;
        }

        public override AssetState Apply(Envelope<IEvent> @event)
        {
            var payload = (SquidexEvent)@event.Payload;

            return Clone().Update(payload, @event.Headers, r => r.DispatchAction(payload));
        }
    }
}
