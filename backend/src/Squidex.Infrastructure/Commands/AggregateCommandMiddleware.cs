// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.Commands;

public class AggregateCommandMiddleware<TCommand, T1> : ICommandMiddleware
    where TCommand : IAggregateCommand where T1 : IAggregate
{
    private readonly IDomainObjectFactory domainObjectFactory;

    public AggregateCommandMiddleware(IDomainObjectFactory domainObjectFactory)
    {
        this.domainObjectFactory = domainObjectFactory;
    }

    public virtual async Task HandleAsync(CommandContext context, NextDelegate next,
        CancellationToken ct)
    {
        await ExecuteCommandAsync(context, ct);

        await next(context, ct);
    }

    protected async Task ExecuteCommandAsync(CommandContext context,
        CancellationToken ct)
    {
        if (context.Command is TCommand typedCommand)
        {
            var commandResult = await ExecuteCommandAsync(typedCommand, ct);
            var commandPayload = await EnrichResultAsync(context, commandResult, ct);

            context.Complete(commandPayload);
        }
    }

    protected virtual Task<object> EnrichResultAsync(CommandContext context, CommandResult result,
        CancellationToken ct)
    {
        return Task.FromResult(result.Payload is None ? result : result.Payload);
    }

    protected virtual Task<CommandResult> ExecuteCommandAsync(TCommand command,
        CancellationToken ct)
    {
        var executable = domainObjectFactory.Create<T1>(command.AggregateId);

        return ExecuteCommandAsync(executable, command, ct);
    }

    protected virtual Task<CommandResult> ExecuteCommandAsync(T1 executable, TCommand command,
        CancellationToken ct)
    {
        return executable.ExecuteAsync(command, ct);
    }
}
