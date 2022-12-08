﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Claims;
using Squidex.Domain.Apps.Events;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.TestHelpers;

public abstract class HandlerTestBase<TState>
{
    private readonly IPersistenceFactory<TState> persistenceFactory = A.Fake<IStore<TState>>();
    private readonly IPersistence<TState> persistence = A.Fake<IPersistence<TState>>();

    protected RefToken Actor { get; } = RefToken.User("me");

    protected RefToken ActorClient { get; } = RefToken.Client("client");

    protected DomainId AppId { get; } = DomainId.NewGuid();

    protected DomainId SchemaId { get; } = DomainId.NewGuid();

    protected string AppName { get; } = "my-app";

    protected string SchemaName { get; } = "my-schema";

    protected ClaimsPrincipal User { get; } = Mocks.FrontendUser();

    protected NamedId<DomainId> AppNamedId
    {
        get => NamedId.Of(AppId, AppName);
    }

    protected NamedId<DomainId> SchemaNamedId
    {
        get => NamedId.Of(SchemaId, SchemaName);
    }

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

        A.CallTo(() => persistence.WriteEventsAsync(A<IReadOnlyList<Envelope<IEvent>>>._, default))
            .Invokes((IReadOnlyList<Envelope<IEvent>> events, CancellationToken _) => LastEvents = events);

        A.CallTo(() => persistence.DeleteAsync(default))
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

    protected async Task<object> PublishIdempotentAsync<T>(DomainObject<T> domainObject, IAggregateCommand command,
        CancellationToken ct = default) where T : class, IDomainState<T>, new()
    {
        var actual = await domainObject.ExecuteAsync(command, default);

        var previousSnapshot = domainObject.Snapshot;
        var previousVersion = domainObject.Snapshot.Version;

        await domainObject.ExecuteAsync(command, ct);

        Assert.Same(previousSnapshot, domainObject.Snapshot);
        Assert.Equal(previousVersion, domainObject.Snapshot.Version);

        return actual.Payload;
    }

    protected TCommand CreateCommand<TCommand>(TCommand command) where TCommand : SquidexCommand
    {
        command.ExpectedVersion = EtagVersion.Any;
        command.Actor ??= Actor;

        if (command.User == null && command.Actor.IsUser)
        {
            command.User = User;
        }

        if (command is IAppCommand { AppId: null } appCommand)
        {
            appCommand.AppId = AppNamedId;
        }

        if (command is ISchemaCommand { SchemaId: null } schemaCommand)
        {
            schemaCommand.SchemaId = SchemaNamedId;
        }

        return command;
    }

    protected TEvent CreateEvent<TEvent>(TEvent @event, bool fromClient = false) where TEvent : SquidexEvent
    {
        @event.Actor = fromClient ? ActorClient : Actor;

        if (@event is AppEvent appEvent)
        {
            appEvent.AppId = AppNamedId;
        }

        if (@event is SchemaEvent schemaEvent)
        {
            schemaEvent.SchemaId = SchemaNamedId;
        }

        return @event;
    }
}
