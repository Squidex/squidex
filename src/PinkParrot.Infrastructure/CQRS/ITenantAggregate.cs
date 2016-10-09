// ==========================================================================
//  ITenantAggregate.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;

namespace PinkParrot.Infrastructure.CQRS
{
    public interface ITenantAggregate : IAggregate
    {
        Guid TenantId { get; }
    }
}
