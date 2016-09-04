// ==========================================================================
//  IStreamNameResolver.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;

namespace PinkParrot.Infrastructure.CQRS.EventStore
{
    public interface IStreamNameResolver
    {
        string GetStreamName(Type aggregateType, Guid id);
    }
}
