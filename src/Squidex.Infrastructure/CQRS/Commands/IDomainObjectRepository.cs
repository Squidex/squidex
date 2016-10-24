// ==========================================================================
//  IDomainObjectRepository.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;

namespace Squidex.Infrastructure.CQRS.Commands
{
    public interface IDomainObjectRepository
    {
        Task<TDomainObject> GetByIdAsync<TDomainObject>(Guid id, int version = int.MaxValue) where TDomainObject : class, IAggregate;

        Task SaveAsync(IAggregate domainObject, Guid commitId);
    }
}