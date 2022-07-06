// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.Commands
{
    public class CachingDomainObjectMiddleware<TCommand, TTarget, TState> : AggregateCommandMiddleware<TCommand, TTarget>
        where TCommand : IAggregateCommand where TTarget : DomainObject<TState> where TState : class, IDomainState<TState>, new()
    {
        private readonly IDomainObjectCache domainObjectCache;

        public CachingDomainObjectMiddleware(IDomainObjectFactory domainObjectFactory, IDomainObjectCache domainObjectCache)
            : base(domainObjectFactory)
        {
            this.domainObjectCache = domainObjectCache;
        }

        protected override async Task<CommandResult> ExecuteCommandAsync(TTarget executable, TCommand command)
        {
            var oldSnapshot = executable.Snapshot;

            var result = await base.ExecuteCommandAsync(executable, command);

            var newSnapshot = executable.Snapshot;

            if (newSnapshot.Version != oldSnapshot.Version)
            {
                await domainObjectCache.SetAsync(executable.UniqueId, oldSnapshot.Version, oldSnapshot);
                await domainObjectCache.SetAsync(executable.UniqueId, newSnapshot.Version, newSnapshot);
            }

            return result;
        }
    }
}
