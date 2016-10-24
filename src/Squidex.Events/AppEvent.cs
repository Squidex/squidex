// ==========================================================================
//  AppEvent.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Squidex.Infrastructure.CQRS.Events;

namespace Squidex.Events
{
    public class AppEvent : IEvent
    {
        public Guid AppId { get; set; }
    }
}
