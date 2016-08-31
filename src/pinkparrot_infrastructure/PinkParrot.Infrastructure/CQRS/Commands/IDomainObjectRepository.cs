// ==========================================================================
//  IDomainObjectRepository.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;

namespace PinkParrot.Infrastructure.CQRS.Commands
{
    public interface IDomainObjectRepository
    {
        Task<TDomainObject> GetByIdAsync<TDomainObject>(Guid id, int version = 0) where TDomainObject : class, IAggregate;

        Task SaveAsync(IAggregate domainObject, Guid commitId);
    }
}