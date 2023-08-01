// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.Commands;

public partial class DomainObject<T>
{
    protected async Task<CommandResult> ApplyReturnAsync<TCommand>(TCommand command, Func<TCommand, CancellationToken, Task<object?>> handler,
        CancellationToken ct = default) where TCommand : ICommand
    {
        return await UpsertCoreAsync(command, handler, ct);
    }

    protected async Task<CommandResult> ApplyReturn<TCommand>(TCommand command, Func<TCommand, object?> handler,
        CancellationToken ct = default) where TCommand : ICommand
    {
        return await UpsertCoreAsync(command, (c, _) =>
        {
            var result = handler(c);
            return Task.FromResult(result);
        }, ct);
    }

    protected async Task<CommandResult> ApplyAsync<TCommand>(TCommand command, Func<TCommand, CancellationToken, Task> handler,
        CancellationToken ct = default) where TCommand : ICommand
    {
        return await UpsertCoreAsync(command, async (c, ct) =>
        {
            await handler(c, ct);
            return None.Value;
        }, ct);
    }

    protected async Task<CommandResult> Apply<TCommand>(TCommand command, Action<TCommand> handler,
        CancellationToken ct = default) where TCommand : ICommand
    {
        Guard.NotNull(handler);

        return await UpsertCoreAsync(command, (c, _) =>
        {
            handler(c);
            return Task.FromResult<object?>(None.Value);
        }, ct);
    }

    protected async Task<CommandResult> DeletePermanentAsync<TCommand>(TCommand command, Func<TCommand, CancellationToken, Task> handler,
        CancellationToken ct = default) where TCommand : ICommand
    {
        Guard.NotNull(handler);

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

        return await DeleteCoreAsync(command, (c, _) =>
        {
            handler(c);

            return Task.FromResult<object?>(None.Value);
        }, ct);
    }
}
