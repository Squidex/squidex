// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Events.Apps
{
    [EventType(nameof(AppClientUpdated), 2)]
    public sealed class AppClientUpdated : AppEvent
    {
        public string Id { get; set; }

        public string? Name { get; set; }

        public string? Role { get; set; }

        public long? ApiCallsLimit { get; set; }

        public long? ApiTrafficLimit { get; set; }

        public bool? AllowAnonymous { get; set; }
    }
}
