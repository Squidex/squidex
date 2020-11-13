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
using Squidex.Infrastructure.States;
using Squidex.Infrastructure.Tasks;
using Squidex.Log;

namespace Squidex.Infrastructure.Commands
{
    public abstract class DomainObjectBase<T> where T : IDomainState<T>, new()
    {
        private readonly List<Envelope<IEvent>> uncomittedEvents = new List<Envelope<IEvent>>();
        private readonly ISemanticLog log;
        private bool isLoaded;
        private DomainId uniqueId;

        public DomainId UniqueId
        {
            get { return uniqueId; }
        }

        public long Version
        {
            get { return Snapshot.Version; }
        }

        public abstract T Snapshot { get; }

        protected DomainObjectBase(ISemanticLog log)
        {
            Guard.NotNull(log, nameof(log));

            this.log = log;
        }

        public virtual void Setup(DomainId uniqueId)
        {
            this.uniqueId = uniqueId;

            OnSetup();
        }

        public virtual async Task EnsureLoadedAsync(bool silent = false)
        {
            if (isLoaded)
            {
                return;
            }

            if (silent)
            {
                await ReadAsync();
            }
            else
            {
                var logContext = (id: uniqueId.ToString(), name: GetType().Name);

                using (log.MeasureInformation(logContext, (ctx, w) => w
                    .WriteProperty("action", "ActivateDomainObject")
                    .WriteProperty("domainObjectType", ctx.name)
                    .WriteProperty("domainObjectKey", ctx.id)))
                {
                    await ReadAsync();
                }
            }

            isLoaded = true;
        }

        protected void RaiseEvent(IEvent @event)
        {
            RaiseEvent(Envelope.Create(@event));
        }

        protected virtual void RaiseEvent(Envelope<IEvent> @event)
        {
            Guard.NotNull(@event, nameof(@event));

            @event.SetAggregateId(uniqueId);

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
            Guard.NotNull(command, nameof(command));
            Guard.NotNull(handler, nameof(handler));

            if (isUpdate)
            {
                await EnsureLoadedAsync();

                if (Version < 0)
                {
                    throw new DomainObjectNotFoundException(uniqueId.ToString());
                }

                if (Version != command.ExpectedVersion && command.ExpectedVersion > EtagVersion.Any)
                {
                    throw new DomainObjectVersionException(uniqueId.ToString(), Version, command.ExpectedVersion);
                }

                if (IsDeleted())
                {
                    throw new DomainException("Object has already been deleted.");
                }

                if (!CanAccept(command))
                {
                    throw new DomainException("Invalid command.");
                }
            }
            else
            {
                command.ExpectedVersion = EtagVersion.Empty;

                if (Version != command.ExpectedVersion)
                {
                    throw new DomainObjectConflictException(uniqueId.ToString());
                }

                if (!CanAcceptCreation(command))
                {
                    throw new DomainException("Invalid command.");
                }
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
                        result = EntityCreatedResult.Create(uniqueId, Version);
                    }
                }

                isLoaded = true;

                return result;
            }
            catch (InconsistentStateException) when (!isUpdate)
            {
                RestorePreviousSnapshot(previousSnapshot, previousVersion);

                throw new DomainObjectConflictException(uniqueId.ToString());
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

        protected virtual bool CanAcceptCreation(ICommand command)
        {
            return true;
        }

        protected virtual bool CanAccept(ICommand command)
        {
            return true;
        }

        protected virtual bool IsDeleted()
        {
            return false;
        }

        protected abstract void RestorePreviousSnapshot(T previousSnapshot, long previousVersion);

        protected abstract bool ApplyEvent(Envelope<IEvent> @event, bool isLoading);

        protected abstract Task ReadAsync();

        protected abstract Task WriteAsync(Envelope<IEvent>[] newEvents, long previousVersion);

        public virtual Task RebuildStateAsync()
        {
            return Task.CompletedTask;
        }

        protected virtual void OnSetup()
        {
        }

        public abstract Task<object?> ExecuteAsync(IAggregateCommand command);
    }
}