// ==========================================================================
//  AppEvent.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Squidex.Infrastructure;

namespace Squidex.Events
{
    public abstract class AppEvent : SquidexEvent
    {
        public NamedId<Guid> AppId { get; set; }
    }
}
