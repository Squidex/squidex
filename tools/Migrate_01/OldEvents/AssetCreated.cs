// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Events.Assets;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Reflection;
using NewAssetCreated = Squidex.Domain.Apps.Events.Assets.AssetCreated;

namespace Migrate_01.OldEvents
{
    [EventType(nameof(AssetCreated))]
    public sealed class AssetCreated : AssetEvent, IMigratedEvent
    {
        public string FileName { get; set; }

        public string MimeType { get; set; }

        public long FileVersion { get; set; }

        public long FileSize { get; set; }

        public bool IsImage { get; set; }

        public int? PixelWidth { get; set; }

        public int? PixelHeight { get; set; }

        public IEvent Migrate()
        {
            return SimpleMapper.Map(this, new NewAssetCreated { Name = FileName });
        }
    }
}
