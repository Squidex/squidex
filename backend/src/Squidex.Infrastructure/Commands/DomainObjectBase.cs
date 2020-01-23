// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Infrastructure.Commands
{
    public abstract class DomainObjectBase<T> where T : IDomainState<T>, new()
    {
        private readonly List<Envelope<IEvent>> uncomittedEvents = new List<Envelope<IEvent>>();
        private readonly ISemanticLog log;
        private bool isLoaded;
        private Guid id;

        public Guid Id
        {
            get { return id; }
        }

        public long Version
        {
            get { return Snapshot.Version; }
        }

        public abstract T Snapshot { get; }

        protected DomainObjectBase(ISemanticLog log)
        {
            Guard.NotNull(log);

            this.log = log;
        }

        public virtual void Setup(Guid id)
        {
            this.id = id;

            OnSetup();
        }

        public virtual async Task EnsureLoadedAsync()
        {
            if (isLoaded)
            {
                return;
            }

            var logContext = (id: id.ToString(), name: GetType().Name);

            using (log.MeasureInformation(logContext, (ctx, w) => w
                .WriteProperty("action", "ActivateDomainObject")
                .WriteProperty("domainObjectType", ctx.name)
                .WriteProperty("domainObjectKey", ctx.id)))
            {
                await ReadAsync();
            }

            isLoaded = true;
        }

        protected void RaiseEvent(IEvent @event)
        {
            RaiseEvent(Envelope.Create(@event));
        }

        protected virtual void RaiseEvent(Envelope<IEvent> @event)
        {
            Guard.NotNull(@event);

            @event.SetAggregateId(id);

            if (ApplyEvent(@event, false))
            {
                uncomittedEvents.Add(@event);
            }
        }

        public IReadOnlyList<Envelope<IEvent>> GetUncomittedEvents()
        {
            return uncomittedEvents;
        }

        public void ClearUncommittedEvents()
        {
            uncomittedEvents.Clear();
        }

        protected Task<object?> CreateReturnAsync<TCommand>(TCommand command, Func<TCommand, Task<object?>> handler) where TCommand : class, IAggregateCommand
        {
            return InvokeAsync(command, handler, false);
        }

        protected Task<object?> CreateReturn<TCommand>(TCommand command, Func<TCommand, object?> handler) where TCommand : class, IAggregateCommand
        {
            return InvokeAsync(command, handler?.ToAsync()!, false);
        }

        protected Task<object?> CreateAsync<TCommand>(TCommand command, Func<TCommand, Task> handler) where TCommand : class, IAggregateCommand
        {
            return InvokeAsync(command, handler.ToDefault<TCommand, object?>(), false);
        }

        protected Task<object?> Create<TCommand>(TCommand command, Action<TCommand> handler) where TCommand : class, IAggregateCommand
        {
            return InvokeAsync(command, handler?.ToDefault<TCommand, object>()?.ToAsync()!, false);
        }

        protected Task<object?> UpdateReturnAsync<TCommand>(TCommand command, Func<TCommand, Task<object?>> handler) where TCommand : class, IAggregateCommand
        {
            return InvokeAsync(command, handler, true);
        }

        protected Task<object?> UpdateReturn<TCommand>(TCommand command, Func<TCommand, object?> handler) where TCommand : class, IAggregateCommand
        {
            return InvokeAsync(command, handler?.ToAsync()!, true);
        }

        protected Task<object?> UpdateAsync<TCommand>(TCommand command, Func<TCommand, Task> handler) where TCommand : class, IAggregateCommand
        {
            return InvokeAsync(command, handler?.ToDefault<TCommand, object>()!, true);
        }

        protected Task<object?> Update<TCommand>(TCommand command, Action<TCommand> handler) where TCommand : class, IAggregateCommand
        {
            return InvokeAsync(command, handler?.ToDefault<TCommand, object>()?.ToAsync()!, true);
        }

        private async Task<object?> InvokeAsync<TCommand>(TCommand command, Func<TCommand, Task<object?>> handler, bool isUpdate) where TCommand : class, IAggregateCommand
        {
            Guard.NotNull(command);
            Guard.NotNull(handler);

            if (isUpdate)
            {
                await EnsureLoadedAsync();
            }

            if (command.ExpectedVersion > EtagVersion.Any && command.ExpectedVersion != Version)
            {
                throw new DomainObjectVersionException(id.ToString(), GetType(), Version, command.ExpectedVersion);
            }

            if (isUpdate == true && Version < 0)
            {
                throw new DomainObjectNotFoundException(id.ToString(), GetType());
            }

            var previousSnapshot = Snapshot;
            var previousVersion = Version;
            try
            {
                var result = await handler(command);

                var events = uncomittedEvents.ToArray();

                await WriteAsync(events, previousVersion);

                if (result == null)
                {
                    if (isUpdate)
                    {
                        result = new EntitySavedResult(Version);
                    }
                    else
                    {
                        result = EntityCreatedResult.Create(id, Version);
                    }
                }

                isLoaded = true;

                return result;
            }
            catch
            {
                RestorePreviousSnapshot(previousSnapshot, previousVersion);

                throw;
            }
            finally
            {
                ClearUncommittedEvents();
            }
        }

        protected abstract void RestorePreviousSnapshot(T previousSnapshot, long previousVersion);

        protected abstract bool ApplyEvent(Envelope<IEvent> @event, bool isLoading);

        protected abstract Task ReadAsync();

        protected abstract Task WriteAsync(Envelope<IEvent>[] newEvents, long previousVersion);

        public virtual Task RebuildStateAsync()
        {
            return TaskHelper.Done;
        }

        protected virtual void OnSetup()
        {
        }

        public abstract Task<object?> ExecuteAsync(IAggregateCommand command);
    }
}