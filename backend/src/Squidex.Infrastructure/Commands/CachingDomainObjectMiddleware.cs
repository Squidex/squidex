// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.Commands;

public class CachingDomainObjectMiddleware<TCommand, T, TState>(IDomainObjectFactory domainObjectFactory, IDomainObjectCache domainObjectCache) : AggregateCommandMiddleware<TCommand, T>(domainObjectFactory)
    where TCommand : IAggregateCommand where T : DomainObject<TState> where TState : Entity, new()
{
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
