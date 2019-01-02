// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Domain.Apps.Events;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Reflection;

namespace Migrate_01.OldEvents
{
    [EventType(nameof(AppClientChanged))]
    [Obsolete]
    public sealed class AppClientChanged : AppEvent, IMigrated<IEvent>
    {
        public string Id { get; set; }

        public bool IsReader { get; set; }

        public IEvent Migrate()
        {
            var permission = IsReader ? AppClientPermission.Reader : AppClientPermission.Editor;

            return SimpleMapper.Map(this, new AppClientUpdated { Permission = permission });
        }
    }
}
