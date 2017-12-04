// ==========================================================================
//  DomainObjectWrapper.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Linq;
using System.Threading.Tasks;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.States;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Infrastructure.Commands
{
    public delegate T DomainObjectFactoryFunction<out T>(Guid id) where T : IAggregate;

    public sealed class DomainObjectWrapper<T> : IStatefulObject where T : IAggregate
    {
        private IPersistence<object> persistence;
        private T domainObject;

        public Task ActivateAsync(string key, IStore store)
        {
            persistence = store.WithEventSourcing<T>(key, e => domainObject.ApplyEvent(e));

            return TaskHelper.Done;
        }

        public Task LoadAsync(T domainObject, long? expectedVersion)
        {
            this.domainObject = domainObject;

            return persistence.ReadAsync(expectedVersion);
        }

        public async Task UpdateAsync(Func<T, Task> handler)
        {
            await handler(domainObject);

            var events = domainObject.GetUncomittedEvents();

            foreach (var @event in events)
            {
                @event.SetAggregateId(domainObject.Id);
            }

            await persistence.WriteEventsAsync(events.ToArray());

            domainObject.ClearUncommittedEvents();
        }
    }
}
