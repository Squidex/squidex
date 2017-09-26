// ==========================================================================
//  DefaultDomainObjectFactory.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;

namespace Squidex.Infrastructure.CQRS.Commands
{
    public delegate T DomainObjectFactoryFunction<out T>(Guid id)
        where T : IAggregate;

    public class DefaultDomainObjectFactory : IDomainObjectFactory
    {
        private readonly IServiceProvider serviceProvider;

        public DefaultDomainObjectFactory(IServiceProvider serviceProvider)
        {
            Guard.NotNull(serviceProvider, nameof(serviceProvider));

            this.serviceProvider = serviceProvider;
        }

        public T CreateNew<T>(Guid id) where T : IAggregate
        {
            var factoryFunction = (DomainObjectFactoryFunction<T>)serviceProvider.GetService(typeof(DomainObjectFactoryFunction<T>));

            if (factoryFunction == null)
            {
                throw new InvalidOperationException($"No factory registered for {typeof(T)}");
            }

            var domainObject = factoryFunction.Invoke(id);

            if (domainObject.Version != -1)
            {
                throw new InvalidOperationException("Must have a version of -1");
            }

            return domainObject;
        }
    }
}
