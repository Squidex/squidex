// ==========================================================================
//  AppMasterLanguageSet.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Events;

namespace Squidex.Events.Apps
{
    [TypeName("AppMasterLanguageSetEvent")]
    public sealed class AppMasterLanguageSet : IEvent
    {
        public Language Language { get; set; }
    }
}
