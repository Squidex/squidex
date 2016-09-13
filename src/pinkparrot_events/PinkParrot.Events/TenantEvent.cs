// ==========================================================================
//  TenantEvent.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;
using PinkParrot.Infrastructure.CQRS.Events;

namespace PinkParrot.Events
{
    public class TenantEvent : IEvent
    {
        public Guid TenantId { get; set; }
    }
}
