// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Events;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Orleans;
using Squidex.Infrastructure.States;

#pragma warning disable IDE0019 // Use pattern matching

namespace Squidex.Domain.Apps.Entities.TestHelpers
{
    public abstract class HandlerTestBase<T, TState> where T : IDomainObjectGrain
    {
        private readonly IStore<Guid> store = A.Fake<IStore<Guid>>();
        private readonly IPersistence<TState> persistence1 = A.Fake<IPersistence<TState>>();
        private readonly IPersistence persistence2 = A.Fake<IPersistence>();

        protected RefToken User { get; } = new RefToken("subject", Guid.NewGuid().ToString());

        protected Guid AppId { get; } = Guid.NewGuid();

        protected Guid SchemaId { get; } = Guid.NewGuid();

        protected string AppName { get; } = "my-app";

        protected string SchemaName { get; } = "my-schema";

        protected NamedId<Guid> AppNamedId
        {
            get { return NamedId.Of(AppId, AppName); }
        }

        protected NamedId<Guid> SchemaNamedId
        {
            get { return NamedId.Of(SchemaId, SchemaName); }
        }

        protected abstract Guid Id { get; }

        public IStore<Guid> Store
        {
            get { return store; }
        }

        public IEnumerable<Envelope<IEvent>> LastEvents { get; private set; } = Enumerable.Empty<Envelope<IEvent>>();

        protected HandlerTestBase()
        {
            A.CallTo(() => store.WithSnapshotsAndEventSourcing(A<Type>.Ignored, Id, A<Func<TState, Task>>.Ignored, A<Func<Envelope<IEvent>, Task>>.Ignored))
                .Returns(persistence1);

            A.CallTo(() => store.WithEventSourcing(A<Type>.Ignored, Id, A<Func<Envelope<IEvent>, Task>>.Ignored))
                .Returns(persistence2);

            A.CallTo(() => persistence1.WriteEventsAsync(A<IEnumerable<Envelope<IEvent>>>.Ignored))
                .Invokes(new Action<IEnumerable<Envelope<IEvent>>>(events =>
                {
                    LastEvents = events;
                }));

            A.CallTo(() => persistence2.WriteEventsAsync(A<IEnumerable<Envelope<IEvent>>>.Ignored))
                .Invokes(new Action<IEnumerable<Envelope<IEvent>>>(events =>
                {
                    LastEvents = events;
                }));
        }

        protected CommandContext CreateContextForCommand<TCommand>(TCommand command) where TCommand : SquidexCommand
        {
            return new CommandContext(CreateCommand(command), A.Dummy<ICommandBus>());
        }

        protected TCommand CreateCommand<TCommand>(TCommand command) where TCommand : SquidexCommand
        {
            command.ExpectedVersion = EtagVersion.Any;

            if (command.Actor == null)
            {
                command.Actor = User;
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

        protected TEvent CreateEvent<TEvent>(TEvent @event) where TEvent : SquidexEvent
        {
            @event.Actor = User;

            EnrichAppInfo(@event);
            EnrichSchemaInfo(@event);

            return @event;
        }

        private void EnrichAppInfo(IEvent @event)
        {
            if (@event is AppEvent appEvent)
            {
                appEvent.AppId = AppNamedId;
            }
        }

        private void EnrichSchemaInfo(IEvent @event)
        {
            if (@event is SchemaEvent schemaEvent)
            {
                schemaEvent.SchemaId = SchemaNamedId;
            }
        }
    }
}

#pragma warning restore IDE0019 // Use pattern matching