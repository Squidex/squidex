// ==========================================================================
//  AutofacDomainObjectFactory.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Autofac;
using Squidex.Infrastructure.CQRS.Commands;

namespace Squidex.Infrastructure.CQRS.Autofac
{
    public sealed class AutofacDomainObjectFactory : IDomainObjectFactory
    {
        private readonly ILifetimeScope lifetimeScope;

        public AutofacDomainObjectFactory(ILifetimeScope lifetimeScope)
        {
            Guard.NotNull(lifetimeScope, nameof(lifetimeScope));

            this.lifetimeScope = lifetimeScope;
        }

        public IAggregate CreateNew(Type type, Guid id)
        {
            return (IAggregate)lifetimeScope.Resolve(type, 
                new NamedParameter("id", id),
                new NamedParameter("version", 0));
        }
    }
}
