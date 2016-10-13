// ==========================================================================
//  AppEvent.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;
using PinkParrot.Infrastructure.CQRS.Events;

namespace PinkParrot.Events
{
    public class AppEvent : IEvent
    {
        public Guid AppId { get; set; }
    }
}
