// ==========================================================================
//  AppClientChanged.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Events.Apps.Old
{
    [EventType(nameof(AppClientChanged))]
    public sealed class AppClientChanged : AppEvent, IMigratedEvent
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
