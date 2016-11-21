// ==========================================================================
//  AppLanguagesConfigured.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Events;

namespace Squidex.Events.Apps
{
    [TypeName("AppLanguagesConfigured")]
    public sealed class AppLanguagesConfigured : IEvent
    {
        public List<Language> Languages { get; set; }
    }
}
