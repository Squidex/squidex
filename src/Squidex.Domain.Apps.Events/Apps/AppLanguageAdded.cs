// ==========================================================================
//  AppLanguageAdded.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Events;

namespace Squidex.Domain.Apps.Events.Apps
{
    [EventType(nameof(AppLanguageAdded))]
    public sealed class AppLanguageAdded : AppEvent
    {
        public Language Language { get; set; }
    }
}
