// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Orleans.Core;
using Squidex.Infrastructure.Orleans;

namespace Squidex.Infrastructure.Commands
{
    public abstract class DomainObjectGrain<T, TState> : GrainBase where T : DomainObject<TState> where TState : class, IDomainState<TState>, new()
    {
        private readonly T domainObject;

        public TState Snapshot
        {
            get => domainObject.Snapshot;
        }

        protected T DomainObject
        {
            get => domainObject;
        }

        protected DomainObjectGrain(IGrainIdentity identity, IDomainObjectFactory factory)
            : base(identity)
        {
            domainObject = factory.Create<T>(DomainId.Create(identity.PrimaryKeyString));
        }

        public Task<CommandResult> ExecuteAsync(IAggregateCommand command)
        {
            return domainObject.ExecuteAsync(command);
        }
    }
}
