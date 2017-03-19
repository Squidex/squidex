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
    public delegate T DomainObjectFactoryFunction<out T>(Guid id) where T : IAggregate;

    public class DefaultDomainObjectFactory : IDomainObjectFactory
    {
        private readonly IServiceProvider serviceProvider;

        public DefaultDomainObjectFactory(IServiceProvider serviceProvider)
        {
            Guard.NotNull(serviceProvider, nameof(serviceProvider));

            this.serviceProvider = serviceProvider;
        }

        public IAggregate CreateNew(Type type, Guid id)
        {
            var factoryFunctionType = typeof(DomainObjectFactoryFunction<>).MakeGenericType(type);
            var factoryFunction = (Delegate)serviceProvider.GetService(factoryFunctionType);

            var aggregate = (IAggregate)factoryFunction.DynamicInvoke(id);

            if (aggregate.Version != -1)
            {
                throw new InvalidOperationException("Must have a version of -1");
            }

            return aggregate;
        }
    }
}
