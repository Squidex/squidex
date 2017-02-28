// ==========================================================================
//  AssertEvent.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Squidex.Infrastructure.CQRS.Events;

namespace Squidex.Events.Assets
{
    public abstract class AssetEvent : IEvent
    {
        public Guid AssetId { get; set; }
    }
}
