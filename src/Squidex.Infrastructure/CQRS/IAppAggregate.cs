// ==========================================================================
//  IAppAggregate.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;

namespace PinkParrot.Infrastructure.CQRS
{
    public interface IAppAggregate : IAggregate
    {
        Guid AppId { get; }
    }
}
