// ==========================================================================
//  AutofacDomainObjectFactory.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;
using Autofac;
using PinkParrot.Infrastructure.CQRS.Commands;

namespace PinkParrot.Infrastructure.CQRS.Autofac
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
