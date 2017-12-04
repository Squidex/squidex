﻿// ==========================================================================
//  AppLanguageUpdated.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Events.Apps
{
    [EventType(nameof(AppLanguageUpdated))]
    public sealed class AppLanguageUpdated : AppEvent
    {
        public Language Language { get; set; }

        public bool IsOptional { get; set; }

        public bool IsMaster { get; set; }

        public List<Language> Fallback { get; set; }
    }
}
