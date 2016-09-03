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
        private readonly IContainer container;

        public AutofacDomainObjectFactory(IContainer container)
        {
            Guard.NotNull(container, nameof(container));

            this.container = container;
        }

        public IAggregate CreateNew(Type type, Guid id)
        {
            return (IAggregate)container.Resolve(type, 
                new NamedParameter("id", id),
                new NamedParameter("version", 0));
        }
    }
}
