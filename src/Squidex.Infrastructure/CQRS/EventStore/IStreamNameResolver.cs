// ==========================================================================
//  IStreamNameResolver.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;

namespace Squidex.Infrastructure.CQRS.EventStore
{
    public interface IStreamNameResolver
    {
        string GetStreamName(Type aggregateType, Guid id);
    }
}
