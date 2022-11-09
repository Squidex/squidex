// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.Commands;

public class CachingDomainObjectMiddleware<TCommand, T, TState> : AggregateCommandMiddleware<TCommand, T>
    where TCommand : IAggregateCommand where T : DomainObject<TState> where TState : class, IDomainState<TState>, new()
{
    private readonly IDomainObjectCache domainObjectCache;

    public CachingDomainObjectMiddleware(IDomainObjectFactory domainObjectFactory, IDomainObjectCache domainObjectCache)
        : base(domainObjectFactory)
    {
        this.domainObjectCache = domainObjectCache;
    }

    protected override async Task<CommandResult> ExecuteCommandAsync(T executable, TCommand command,
        CancellationToken ct)
    {
        var oldSnapshot = executable.Snapshot;

        var result = await base.ExecuteCommandAsync(executable, command, ct);

        var newSnapshot = executable.Snapshot;

        if (newSnapshot.Version != oldSnapshot.Version)
        {
            // If we are so far it is not worth to cancel the flow anymore.
            await domainObjectCache.SetAsync(executable.UniqueId, oldSnapshot.Version, oldSnapshot, default);
            await domainObjectCache.SetAsync(executable.UniqueId, newSnapshot.Version, newSnapshot, default);
        }

        return result;
    }
}
