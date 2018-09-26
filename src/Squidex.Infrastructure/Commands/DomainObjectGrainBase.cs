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
using Squidex.Infrastructure.Orleans;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Infrastructure.Commands
{
    public abstract class DomainObjectGrainBase<T> : GrainOfGuid, IDomainObjectGrain where T : IDomainState, new()
    {
        private readonly List<Envelope<IEvent>> uncomittedEvents = new List<Envelope<IEvent>>();
        private readonly ISemanticLog log;
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

        protected DomainObjectGrainBase(ISemanticLog log)
        {
            Guard.NotNull(log, nameof(log));

            this.log = log;
        }

        public sealed override async Task OnActivateAsync(Guid key)
        {
            using (log.MeasureInformation(w => w
                .WriteProperty("action", "ActivateDomainObject")
                .WriteProperty("domainObjectType", GetType().Name)
                .WriteProperty("domainObjectKey", key.ToString())))
            {
                id = key;

                await ReadAsync(GetType(), id);
            }
        }

        public void RaiseEvent(IEvent @event)
        {
            RaiseEvent(Envelope.Create(@event));
        }

        public virtual void RaiseEvent(Envelope<IEvent> @event)
        {
            Guard.NotNull(@event, nameof(@event));

            @event.SetAggregateId(id);

            ApplyEvent(@event);

            uncomittedEvents.Add(@event);
        }

        public IReadOnlyList<Envelope<IEvent>> GetUncomittedEvents()
        {
            return uncomittedEvents;
        }

        public void ClearUncommittedEvents()
        {
            uncomittedEvents.Clear();
        }

        protected Task<object> CreateReturnAsync<TCommand>(TCommand command, Func<TCommand, Task<object>> handler) where TCommand : class, IAggregateCommand
        {
            return InvokeAsync(command, handler, false);
        }

        protected Task<object> CreateReturnAsync<TCommand>(TCommand command, Func<TCommand, object> handler) where TCommand : class, IAggregateCommand
        {
            return InvokeAsync(command, handler?.ToAsync(), false);
        }

        protected Task<object> CreateAsync<TCommand>(TCommand command, Func<TCommand, Task> handler) where TCommand : class, IAggregateCommand
        {
            return InvokeAsync(command, handler.ToDefault<TCommand, object>(), false);
        }

        protected Task<object> CreateAsync<TCommand>(TCommand command, Action<TCommand> handler) where TCommand : class, IAggregateCommand
        {
            return InvokeAsync(command, handler?.ToDefault<TCommand, object>()?.ToAsync(), false);
        }

        protected Task<object> UpdateReturnAsync<TCommand>(TCommand command, Func<TCommand, Task<object>> handler) where TCommand : class, IAggregateCommand
        {
            return InvokeAsync(command, handler, true);
        }

        protected Task<object> UpdateAsync<TCommand>(TCommand command, Func<TCommand, object> handler) where TCommand : class, IAggregateCommand
        {
            return InvokeAsync(command, handler?.ToAsync(), true);
        }

        protected Task<object> UpdateAsync<TCommand>(TCommand command, Func<TCommand, Task> handler) where TCommand : class, IAggregateCommand
        {
            return InvokeAsync(command, handler?.ToDefault<TCommand, object>(), true);
        }

        protected Task<object> UpdateAsync<TCommand>(TCommand command, Action<TCommand> handler) where TCommand : class, IAggregateCommand
        {
            return InvokeAsync(command, handler?.ToDefault<TCommand, object>()?.ToAsync(), true);
        }

        private async Task<object> InvokeAsync<TCommand>(TCommand command, Func<TCommand, Task<object>> handler, bool isUpdate) where TCommand : class, IAggregateCommand
        {
            Guard.NotNull(command, nameof(command));

            if (command.ExpectedVersion != EtagVersion.Any && command.ExpectedVersion != Version)
            {
                throw new DomainObjectVersionException(id.ToString(), GetType(), Version, command.ExpectedVersion);
            }

            if (isUpdate && Version < 0)
            {
                try
                {
                    DeactivateOnIdle();
                }
                catch (InvalidOperationException)
                {
                }

                throw new DomainObjectNotFoundException(id.ToString(), GetType());
            }

            if (!isUpdate && Version >= 0)
            {
                throw new DomainException("Object has already been created.");
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

                return result;
            }
            catch
            {
                RestorePreviousSnapshot(previousSnapshot, previousVersion);

                throw;
            }
            finally
            {
                uncomittedEvents.Clear();
            }
        }

        protected abstract void RestorePreviousSnapshot(T previousSnapshot, long previousVersion);

        protected abstract void ApplyEvent(Envelope<IEvent> @event);

        protected abstract Task ReadAsync(Type type, Guid id);

        protected abstract Task WriteAsync(Envelope<IEvent>[] events, long previousVersion);

        public async Task<J<object>> ExecuteAsync(J<IAggregateCommand> command)
        {
            var result = await ExecuteAsync(command.Value);

            return result.AsJ();
        }

        protected abstract Task<object> ExecuteAsync(IAggregateCommand command);
    }
}