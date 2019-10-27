// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Events.Apps
{
    [EventType(nameof(AppRoleUpdated))]
    public sealed class AppRoleUpdated : AppEvent
    {
        public string Name { get; set; }

        public string[] Permissions { get; set; }
    }
}
