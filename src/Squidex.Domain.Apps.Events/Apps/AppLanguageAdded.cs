// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Events.Apps
{
    [EventType(nameof(AppLanguageAdded))]
    public sealed class AppLanguageAdded : AppEvent
    {
        public Language Language { get; set; }
    }
}
