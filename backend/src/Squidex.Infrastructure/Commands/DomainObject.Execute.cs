// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.Commands;

public partial class DomainObject<T>
{
    protected Task<CommandResult> CreateReturnAsync<TCommand>(TCommand command, Func<TCommand, CancellationToken, Task<object?>> handler,
        CancellationToken ct = default) where TCommand : ICommand
    {
        EnsureCanCreate(command);

        return UpsertCoreAsync(command, handler, true, ct);
    }

    protected Task<CommandResult> CreateReturn<TCommand>(TCommand command, Func<TCommand, object?> handler,
        CancellationToken ct = default) where TCommand : ICommand
    {
        return CreateReturnAsync(command, (c, _) =>
        {
            var result = handler(c);

            return Task.FromResult(result);
        }, ct);
    }

    protected Task<CommandResult> CreateAsync<TCommand>(TCommand command, Func<TCommand, CancellationToken, Task> handler,
        CancellationToken ct = default) where TCommand : ICommand
    {
        EnsureCanCreate(command);

        return UpsertCoreAsync(command, async (c, ct) =>
        {
            await handler(c, ct);

            return None.Value;
        }, true, ct);
    }

    protected Task<CommandResult> Create<TCommand>(TCommand command, Action<TCommand> handler,
        CancellationToken ct = default) where TCommand : ICommand
    {
        return CreateAsync(command, (c, ct) =>
        {
            handler(c);

            return Task.FromResult<object?>(None.Value);
        }, ct);
    }

    protected async Task<CommandResult> UpdateReturnAsync<TCommand>(TCommand command, Func<TCommand, CancellationToken, Task<object?>> handler,
        CancellationToken ct = default) where TCommand : ICommand
    {
        await EnsureCanUpdateAsync(command, ct);

        return await UpsertCoreAsync(command, handler, false, ct);
    }

    protected Task<CommandResult> UpdateReturn<TCommand>(TCommand command, Func<TCommand, object?> handler,
        CancellationToken ct = default) where TCommand : ICommand
    {
        return UpdateReturnAsync(command, (c, ct) =>
        {
            var result = handler(c);

            return Task.FromResult(result);
        }, ct);
    }

    protected async Task<CommandResult> UpdateAsync<TCommand>(TCommand command, Func<TCommand, CancellationToken, Task> handler,
        CancellationToken ct = default) where TCommand : ICommand
    {
        await EnsureCanUpdateAsync(command, ct);

        return await UpsertCoreAsync(command, async (c, ct) =>
        {
            await handler(c, ct);

            return None.Value;
        }, false, ct);
    }

    protected async Task<CommandResult> Update<TCommand>(TCommand command, Action<TCommand> handler,
        CancellationToken ct = default) where TCommand : ICommand
    {
        return await UpdateAsync(command, (c, _) =>
        {
            handler(c);

            return Task.FromResult<object?>(None.Value);
        }, ct);
    }

    protected async Task<CommandResult> UpsertReturnAsync<TCommand>(TCommand command, Func<TCommand, CancellationToken, Task<object?>> handler,
        CancellationToken ct = default) where TCommand : ICommand
    {
        await EnsureCanUpsertAsync(command, ct);

        return await UpsertCoreAsync(command, handler, true, ct);
    }

    protected async Task<CommandResult> UpsertReturn<TCommand>(TCommand command, Func<TCommand, object?> handler,
        CancellationToken ct = default) where TCommand : ICommand
    {
        return await UpsertReturnAsync(command, (c, _) =>
        {
            var result = handler(c);

            return Task.FromResult(result);
        }, ct);
    }

    protected async Task<CommandResult> UpsertAsync<TCommand>(TCommand command, Func<TCommand, CancellationToken, Task> handler,
        CancellationToken ct = default) where TCommand : ICommand
    {
        await EnsureCanUpsertAsync(command, ct);

        return await UpsertCoreAsync(command, async (c, ct) =>
        {
            await handler(c, ct);

            return None.Value;
        }, true, ct);
    }

    protected async Task<CommandResult> Upsert<TCommand>(TCommand command, Action<TCommand> handler,
        CancellationToken ct = default) where TCommand : ICommand
    {
        Guard.NotNull(handler);

        return await UpsertAsync(command, (c, _) =>
        {
            handler(c);

            return Task.FromResult<object?>(None.Value);
        }, ct);
    }

    protected async Task<CommandResult> DeletePermanentAsync<TCommand>(TCommand command, Func<TCommand, CancellationToken, Task> handler,
        CancellationToken ct = default) where TCommand : ICommand
    {
        Guard.NotNull(handler);

        await EnsureCanDeleteAsync(command, ct);

        return await DeleteCoreAsync(command, async (c, ct) =>
        {
            await handler(c, ct);

            return None.Value;
        }, ct);
    }

    protected async Task<CommandResult> DeletePermanent<TCommand>(TCommand command, Action<TCommand> handler,
        CancellationToken ct = default) where TCommand : ICommand
    {
        Guard.NotNull(handler);

        return await DeletePermanentAsync(command, (c, _) =>
        {
            handler(c);

            return Task.FromResult<object?>(None.Value);
        }, ct);
    }

    private void EnsureCanCreate<TCommand>(TCommand command) where TCommand : ICommand
    {
        Guard.NotNull(command);

        if (Version != EtagVersion.Empty && !(IsDeleted(Snapshot) && CanRecreate()))
        {
            throw new DomainObjectConflictException(uniqueId.ToString());
        }

        MatchingVersion(command);
        MatchingCreateCommand(command);
    }

    private async Task EnsureCanUpdateAsync<TCommand>(TCommand command,
        CancellationToken ct) where TCommand : ICommand
    {
        Guard.NotNull(command);

        await EnsureLoadedAsync(ct);

        NotDeleted();
        NotEmpty();

        MatchingVersion(command);
        MatchingCommand(command);
    }

    private async Task EnsureCanUpsertAsync<TCommand>(TCommand command,
        CancellationToken ct) where TCommand : ICommand
    {
        Guard.NotNull(command);

        await EnsureLoadedAsync(ct);

        if (IsDeleted(Snapshot) && !CanRecreate())
        {
            throw new DomainObjectDeletedException(uniqueId.ToString());
        }

        MatchingVersion(command);

        if (Version <= EtagVersion.Empty)
        {
            MatchingCreateCommand(command);
        }
        else
        {
            MatchingCommand(command);
        }
    }

    private async Task EnsureCanDeleteAsync<TCommand>(TCommand command,
        CancellationToken ct) where TCommand : ICommand
    {
        Guard.NotNull(command);

        await EnsureLoadedAsync(ct);

        NotEmpty();

        MatchingVersion(command);
        MatchingCommand(command);
    }

    private void NotDeleted()
    {
        if (IsDeleted(Snapshot))
        {
            throw new DomainObjectDeletedException(uniqueId.ToString());
        }
    }

    private void NotEmpty()
    {
        if (Version <= EtagVersion.Empty)
        {
            throw new DomainObjectNotFoundException(uniqueId.ToString());
        }
    }

    private void MatchingVersion<TCommand>(TCommand command) where TCommand : ICommand
    {
        if (Version > EtagVersion.Empty && command.ExpectedVersion > EtagVersion.Any && Version != command.ExpectedVersion)
        {
            throw new DomainObjectVersionException(uniqueId.ToString(), Version, command.ExpectedVersion);
        }
    }

    private void MatchingCreateCommand<TCommand>(TCommand command) where TCommand : ICommand
    {
        if (!CanAcceptCreation(command))
        {
            throw new DomainException("Invalid command.");
        }
    }

    private void MatchingCommand<TCommand>(TCommand command) where TCommand : ICommand
    {
        if (!CanAccept(command))
        {
            throw new DomainException("Invalid command.");
        }
    }
}
