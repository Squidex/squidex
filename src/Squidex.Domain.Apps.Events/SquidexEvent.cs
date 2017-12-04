// ==========================================================================
//  SquidexEvent.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Events
{
    public abstract class SquidexEvent : IEvent
    {
        public string Username { get; set; }

        public RefToken Actor { get; set; }
    }
}
