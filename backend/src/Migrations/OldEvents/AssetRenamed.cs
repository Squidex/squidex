// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Migrations;
using Squidex.Infrastructure.Reflection;

namespace Migrations.OldEvents
{
    [EventType(nameof(AssetRenamed))]
    [Obsolete("New Event introduced")]
    public sealed class AssetRenamed : AssetEvent, IMigrated<IEvent>
    {
        public string FileName { get; set; }

        public IEvent Migrate()
        {
            return SimpleMapper.Map(this, new AssetAnnotated());
        }
    }
}
