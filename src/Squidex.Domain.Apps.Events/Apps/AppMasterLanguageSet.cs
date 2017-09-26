// ==========================================================================
//  AppMasterLanguageSet.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Events;

namespace Squidex.Domain.Apps.Events.Apps
{
    [EventType(nameof(AppMasterLanguageSet))]
    public sealed class AppMasterLanguageSet : AppEvent
    {
        public Language Language { get; set; }
    }
}
