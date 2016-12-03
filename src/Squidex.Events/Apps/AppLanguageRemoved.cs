// ==========================================================================
//  AppLanguageRemoved.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Events;

namespace Squidex.Events.Apps
{
    [TypeName("AppLanguageRemovedEvent")]
    public sealed class AppLanguageRemoved : IEvent
    {
        public Language Language { get; set; }
    }
}
