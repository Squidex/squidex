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
        Task<TDomainObject> GetByIdAsync<TDomainObject>(Guid id, int version = int.MaxValue) where TDomainObject : class, IAggregate;

        Task SaveAsync(IAggregate domainObject, ICollection<Envelope<IEvent>> events, Guid commitId);
    }
}