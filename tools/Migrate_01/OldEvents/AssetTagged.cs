﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Migrations;
using Squidex.Infrastructure.Reflection;

namespace Migrate_01.OldEvents
{
    [EventType(nameof(AssetTagged))]
    [Obsolete]
    public sealed class AssetTagged : AssetEvent, IMigrated<IEvent>
    {
        public HashSet<string> Tags { get; set; }

        public IEvent Migrate()
        {
            return SimpleMapper.Map(this, new AssetAnnotated());
        }
    }
}
