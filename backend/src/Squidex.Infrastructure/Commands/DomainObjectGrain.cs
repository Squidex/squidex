// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Infrastructure.Orleans;

namespace Squidex.Infrastructure.Commands
{
    public class DomainObjectGrain<T, TState> : GrainOfGuid
        where T : DomainObjectBase<TState>
        where TState : class, IDomainState<TState>, new()
    {
        private readonly T domainObject;

        public TState Snapshot
        {
            get { return domainObject.Snapshot; }
        }

        protected T DomainObject
        {
            get { return domainObject; }
        }

        public DomainObjectGrain(IServiceProvider serviceProvider)
        {
            Guard.NotNull(serviceProvider);

            domainObject = (serviceProvider.GetService(typeof(T)) as T)!;
        }

        protected override Task OnActivateAsync(Guid key)
        {
            domainObject.Setup(key);

            return base.OnActivateAsync(key);
        }

        public async Task<J<object?>> ExecuteAsync(J<IAggregateCommand> command)
        {
            var result = await domainObject.ExecuteAsync(command.Value);

            return result;
        }
    }
}