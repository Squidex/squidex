// ==========================================================================
//  IDomainObjectRepository.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Infrastructure.CQRS.Events;

namespace Squidex.Infrastructure.CQRS.Commands
{
    public interface IDomainObjectRepository
    {
        Task LoadAsync(IAggregate domainObject, long? expectedVersion = null);

        Task SaveAsync(IAggregate domainObject, ICollection<Envelope<IEvent>> events, Guid commitId);
    }
}