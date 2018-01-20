﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Events;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.States;

#pragma warning disable IDE0019 // Use pattern matching

namespace Squidex.Domain.Apps.Entities.TestHelpers
{
    public abstract class HandlerTestBase<T> where T : IDomainObject
    {
        private sealed class MockupHandler : IAggregateHandler
        {
            private T domainObject;

            public bool IsCreated { get; private set; }
            public bool IsUpdated { get; private set; }

            public void Init(T newDomainObject)
            {
                domainObject = newDomainObject;

                IsCreated = false;
                IsUpdated = false;
            }

            public Task<V> CreateSyncedAsync<V>(CommandContext context, Func<V, Task> creator) where V : class, IDomainObject
            {
                return CreateAsync(context, creator);
            }

            public Task<V> UpdateSyncedAsync<V>(CommandContext context, Func<V, Task> creator) where V : class, IDomainObject
            {
                return UpdateAsync(context, creator);
            }

            public async Task<V> CreateAsync<V>(CommandContext context, Func<V, Task> creator) where V : class, IDomainObject
            {
                IsCreated = true;

                var @do = domainObject as V;

                await creator(domainObject as V);

                return @do;
            }

            public async Task<V> UpdateAsync<V>(CommandContext context, Func<V, Task> updater) where V : class, IDomainObject
            {
                IsUpdated = true;

                var @do = domainObject as V;

                await updater(domainObject as V);

                return @do;
            }
        }

        private readonly MockupHandler handler = new MockupHandler();

        protected RefToken User { get; } = new RefToken("subject", Guid.NewGuid().ToString());

        protected Guid AppId { get; } = Guid.NewGuid();

        protected Guid SchemaId { get; } = Guid.NewGuid();

        protected abstract Guid Id { get; }

        protected string AppName { get; } = "my-app";

        protected string SchemaName { get; } = "my-schema";

        protected NamedId<Guid> AppNamedId
        {
            get { return new NamedId<Guid>(AppId, AppName); }
        }

        protected NamedId<Guid> SchemaNamedId
        {
            get { return new NamedId<Guid>(SchemaId, SchemaName); }
        }

        protected IAggregateHandler Handler
        {
            get { return handler; }
        }

        protected CommandContext CreateContextForCommand<TCommand>(TCommand command) where TCommand : SquidexCommand
        {
            return new CommandContext(CreateCommand(command));
        }

        protected async Task TestCreate(T domainObject, Func<T, Task> action, bool shouldCreate = true)
        {
            handler.Init(domainObject);

            await domainObject.ActivateAsync(Id, A.Fake<IStore<Guid>>());
            await action(domainObject);

            if (!handler.IsCreated && shouldCreate)
            {
                throw new InvalidOperationException("Create not called.");
            }
        }

        protected async Task TestUpdate(T domainObject, Func<T, Task> action, bool shouldUpdate = true)
        {
            handler.Init(domainObject);

            await domainObject.ActivateAsync(Id, A.Fake<IStore<Guid>>());
            await action(domainObject);

            if (!handler.IsUpdated && shouldUpdate)
            {
                throw new InvalidOperationException("Update not called.");
            }
        }

        protected TCommand CreateCommand<TCommand>(TCommand command) where TCommand : SquidexCommand
        {
            if (command.Actor == null)
            {
                command.Actor = User;
            }

            var appCommand = command as AppCommand;

            if (appCommand != null && appCommand.AppId == null)
            {
                appCommand.AppId = AppNamedId;
            }

            var schemaCommand = command as SchemaCommand;

            if (schemaCommand != null && schemaCommand.SchemaId == null)
            {
                schemaCommand.SchemaId = SchemaNamedId;
            }

            return command;
        }

        protected TEvent CreateEvent<TEvent>(TEvent @event) where TEvent : SquidexEvent
        {
            @event.Actor = User;

            var appEvent = @event as AppEvent;

            if (appEvent != null)
            {
                appEvent.AppId = AppNamedId;
            }

            var schemaEvent = @event as SchemaEvent;

            if (schemaEvent != null)
            {
                schemaEvent.SchemaId = SchemaNamedId;
            }

            return @event;
        }
    }
}

#pragma warning restore IDE0019 // Use pattern matching