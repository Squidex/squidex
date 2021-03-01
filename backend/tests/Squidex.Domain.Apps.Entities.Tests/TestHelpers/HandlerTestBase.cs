﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Events;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Orleans;
using Squidex.Infrastructure.States;
using Xunit;

namespace Squidex.Domain.Apps.Entities.TestHelpers
{
    public abstract class HandlerTestBase<TState>
    {
        private readonly IStore<DomainId> store = A.Fake<IStore<DomainId>>();
        private readonly IPersistence<TState> persistenceWithState = A.Fake<IPersistence<TState>>();
        private readonly IPersistence persistence = A.Fake<IPersistence>();

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

        public IStore<DomainId> Store
        {
            get => store;
        }

        public IEnumerable<Envelope<IEvent>> LastEvents { get; private set; } = Enumerable.Empty<Envelope<IEvent>>();

        protected HandlerTestBase()
        {
            A.CallTo(() => store.WithSnapshotsAndEventSourcing(A<Type>._, Id, A<HandleSnapshot<TState>>._, A<HandleEvent>._))
                .Returns(persistenceWithState);

            A.CallTo(() => store.WithEventSourcing(A<Type>._, Id, A<HandleEvent>._))
                .Returns(persistence);

            A.CallTo(() => persistenceWithState.WriteEventsAsync(A<IReadOnlyList<Envelope<IEvent>>>._))
                .Invokes((IReadOnlyList<Envelope<IEvent>> events) => LastEvents = events);

            A.CallTo(() => persistence.WriteEventsAsync(A<IReadOnlyList<Envelope<IEvent>>>._))
                .Invokes((IReadOnlyList<Envelope<IEvent>> events) => LastEvents = events);

            A.CallTo(() => persistenceWithState.DeleteAsync())
                .Invokes(() => LastEvents = Enumerable.Empty<Envelope<IEvent>>());

            A.CallTo(() => persistence.DeleteAsync())
                .Invokes(() => LastEvents = Enumerable.Empty<Envelope<IEvent>>());
        }

        protected CommandContext CreateCommandContext<TCommand>(TCommand command) where TCommand : SquidexCommand
        {
            return new CommandContext(CreateCommand(command), A.Dummy<ICommandBus>());
        }

        protected async Task<CommandContext> HandleAsync<TCommand>(ICommandMiddleware middleware, TCommand command) where TCommand : SquidexCommand
        {
            var context = new CommandContext(CreateCommand(command), A.Dummy<ICommandBus>());

            await middleware.HandleAsync(context);

            return context;
        }

        protected async Task<object> PublishIdempotentAsync<T>(DomainObject<T> domainObject, IAggregateCommand command) where T : class, IDomainState<T>, new()
        {
            var result = await domainObject.ExecuteAsync(command);

            var previousSnapshot = domainObject.Snapshot;
            var previousVersion = domainObject.Snapshot.Version;

            await domainObject.ExecuteAsync(command);

            Assert.Same(previousSnapshot, domainObject.Snapshot);
            Assert.Equal(previousVersion, domainObject.Snapshot.Version);

            return result.Payload;
        }

        protected TCommand CreateCommand<TCommand>(TCommand command) where TCommand : SquidexCommand
        {
            command.ExpectedVersion = EtagVersion.Any;
            command.Actor ??= Actor;

            if (command.User == null && command.Actor.IsUser)
            {
                command.User = User;
            }

            if (command is IAppCommand appCommand && appCommand.AppId == null)
            {
                appCommand.AppId = AppNamedId;
            }

            if (command is ISchemaCommand schemaCommand && schemaCommand.SchemaId == null)
            {
                schemaCommand.SchemaId = SchemaNamedId;
            }

            return command;
        }

        protected static J<IAggregateCommand> J(IAggregateCommand command)
        {
            return command.AsJ();
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
}