// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Events.Assets;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Reflection;
using NewAssetRenamed = Squidex.Domain.Apps.Events.Assets.AssetRenamed;

namespace Migrate_01.OldEvents
{
    [EventType(nameof(AssetRenamed))]
    public sealed class AssetRenamed : AssetEvent, IMigratedEvent
    {
        public string FileName { get; set; }

        public IEvent Migrate()
        {
            return SimpleMapper.Map(this, new NewAssetRenamed { Name = FileName });
        }
    }
}
