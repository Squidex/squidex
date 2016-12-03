// ==========================================================================
//  AppLanguageAdded.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Events;

namespace Squidex.Events.Apps
{
    [TypeName("AppLanguageAddedEvent")]
    public sealed class AppLanguageAdded : IEvent
    {
        public Language Language { get; set; }
    }
}
