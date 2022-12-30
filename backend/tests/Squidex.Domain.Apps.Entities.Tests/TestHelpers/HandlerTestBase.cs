// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Teams;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.TestHelpers;

public abstract class HandlerTestBase<TState> : GivenContext
{
    private readonly IPersistenceFactory<TState> persistenceFactory = A.Fake<IStore<TState>>();
    private readonly IPersistence<TState> persistence = A.Fake<IPersistence<TState>>();

    protected abstract DomainId Id { get; }

    public IPersistenceFactory<TState> PersistenceFactory
    {
        get => persistenceFactory;
    }

    public IEnumerable<Envelope<IEvent>> LastEvents { get; private set; } = Enumerable.Empty<Envelope<IEvent>>();

    protected HandlerTestBase()
    {
#pragma warning disable MA0056 // Do not call overridable members in constructor
        A.CallTo(() => persistenceFactory.WithSnapshotsAndEventSourcing(A<Type>._, Id, A<HandleSnapshot<TState>>._, A<HandleEvent>._))
            .Returns(persistence);

        A.CallTo(() => persistenceFactory.WithEventSourcing(A<Type>._, Id, A<HandleEvent>._))
            .Returns(persistence);

        A.CallTo(() => persistence.WriteEventsAsync(A<IReadOnlyList<Envelope<IEvent>>>._, CancellationToken))
            .Invokes((IReadOnlyList<Envelope<IEvent>> events, CancellationToken _) => LastEvents = events);

        A.CallTo(() => persistence.DeleteAsync(CancellationToken))
            .Invokes(() => LastEvents = Enumerable.Empty<Envelope<IEvent>>());
#pragma warning restore MA0056 // Do not call overridable members in constructor
    }

    protected CommandContext CreateCommandContext<TCommand>(TCommand command) where TCommand : SquidexCommand
    {
        return new CommandContext(CreateCommand(command), A.Dummy<ICommandBus>());
    }

    protected async Task<CommandContext> HandleAsync<TCommand>(ICommandMiddleware middleware, TCommand command,
        CancellationToken ct = default) where TCommand : SquidexCommand
    {
        var context = new CommandContext(CreateCommand(command), A.Dummy<ICommandBus>());

        await middleware.HandleAsync(context, ct);

        return context;
    }

    protected async Task<object> PublishIdempotentAsync<T>(DomainObject<T> domainObject, IAggregateCommand command) where T : class, IDomainState<T>, new()
    {
        var actual = await domainObject.ExecuteAsync(command, CancellationToken);

        var previousSnapshot = domainObject.Snapshot;
        var previousVersion = domainObject.Snapshot.Version;

        await domainObject.ExecuteAsync(command, CancellationToken);

        Assert.Same(previousSnapshot, domainObject.Snapshot);
        Assert.Equal(previousVersion, domainObject.Snapshot.Version);

        return actual.Payload;
    }

    protected TCommand CreateCommand<TCommand>(TCommand command) where TCommand : SquidexCommand
    {
        command.ExpectedVersion = EtagVersion.Any;
        command.Actor ??= User;

        if (command.User == null && command.Actor.IsUser)
        {
            command.User = ApiContext.UserPrincipal;
        }

        if (command is IAppCommand { AppId: null } appCommand)
        {
            appCommand.AppId = AppId;
        }

        if (command is ISchemaCommand { SchemaId: null } schemaCommand)
        {
            schemaCommand.SchemaId = SchemaId;
        }

        if (command is ITeamCommand teamCommand && teamCommand.TeamId == default)
        {
            teamCommand.TeamId = TeamId;
        }

        return command;
    }

    protected TEvent CreateEvent<TEvent>(TEvent @event, bool fromClient = false) where TEvent : SquidexEvent
    {
        @event.Actor = fromClient ? Client : User;

        if (@event is AppEvent appEvent)
        {
            appEvent.AppId = AppId;
        }

        if (@event is SchemaEvent schemaEvent)
        {
            schemaEvent.SchemaId = SchemaId;
        }

        if (@event is TeamEvent teamEvent && teamEvent.TeamId == default)
        {
            teamEvent.TeamId = TeamId;
        }

        return @event;
    }
}
