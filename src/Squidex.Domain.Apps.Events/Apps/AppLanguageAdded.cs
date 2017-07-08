// ==========================================================================
//  AppLanguageAdded.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Events.Apps
{
    [TypeName("AppLanguageAddedEvent")]
    public sealed class AppLanguageAdded : AppEvent
    {
        public Language Language { get; set; }
    }
}
