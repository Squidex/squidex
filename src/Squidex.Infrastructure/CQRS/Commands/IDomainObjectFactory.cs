// ==========================================================================
//  IDomainObjectFactory.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;

namespace Squidex.Infrastructure.CQRS.Commands
{
    public interface IDomainObjectFactory
    {
        IAggregate CreateNew(Type type, Guid id);
    }
}
