// ==========================================================================
//  HandlerTestBase.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Infrastructure.CQRS;
using Squidex.Infrastructure.CQRS.Commands;

namespace Squidex.Write.Utils
{
    public abstract class HandlerTestBase<T> where T : DomainObject
    {
        private sealed class MockupHandler : IAggregateHandler
        {
            private T domainObject;

            public bool IsCreated { get; private set; }
            public bool IsUpdated { get; private set; }

            public void Init(T newDomainObject)
            {
                domainObject = newDomainObject;

                IsCreated = false;
                IsUpdated = false;
            }

            public Task CreateAsync<V>(IAggregateCommand command, Func<V, Task> creator) where V : class, IAggregate
            {
                IsCreated = true;

                return creator(domainObject as V);
            }

            public Task UpdateAsync<V>(IAggregateCommand command, Func<V, Task> updater) where V : class, IAggregate
            {
                IsUpdated = true;

                return updater(domainObject as V);
            }
        }

        private readonly MockupHandler handler = new MockupHandler();
        private readonly Guid id = Guid.NewGuid();

        protected Guid Id
        {
            get { return id; }
        }

        protected IAggregateHandler Handler
        {
            get { return handler; }
        }

        public async Task TestCreate(T domainObject, Func<T, Task> action, bool shouldCreate = true)
        {
            handler.Init(domainObject);

            await action(domainObject);

            if (!handler.IsCreated && shouldCreate)
            {
                throw new InvalidOperationException("Create not called");
            }
        }

        public async Task TestUpdate(T domainObject, Func<T, Task> action, bool shouldUpdate = true)
        {
            handler.Init(domainObject);

            await action(domainObject);

            if (!handler.IsUpdated && shouldUpdate)
            {
                throw new InvalidOperationException("Create not called");
            }
        }
    }
}
