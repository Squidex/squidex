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
using Squidex.Log;

namespace Squidex.Infrastructure.Commands
{
    public abstract class DomainObjectBase<T> where T : IDomainState<T>, new()
    {
        private readonly List<Envelope<IEvent>> uncomittedEvents = new List<Envelope<IEvent>>();
        private readonly ISemanticLog log;
        private bool isLoaded;
        private DomainId uniqueId;

        private enum Mode
        {
            Create,
            Update,
            Upsert
        }

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

        protected Task<CommandResult> CreateReturnAsync<TCommand>(TCommand command, Func<TCommand, Task<object?>> handler) where TCommand : class, IAggregateCommand
        {
            Guard.NotNull(handler, nameof(handler));

            return InvokeAsync(command, handler, Mode.Create);
        }

        protected Task<CommandResult> CreateReturn<TCommand>(TCommand command, Func<TCommand, object?> handler) where TCommand : class, IAggregateCommand
        {
            Guard.NotNull(handler, nameof(handler));

            return InvokeAsync(command, x =>
            {
                var result = handler(x);

                return Task.FromResult(result);
            }, Mode.Create);
        }

        protected Task<CommandResult> CreateAsync<TCommand>(TCommand command, Func<TCommand, Task> handler) where TCommand : class, IAggregateCommand
        {
            Guard.NotNull(handler, nameof(handler));

            return InvokeAsync(command, async x =>
            {
                await handler(x);

                return None.Value;
            }, Mode.Create);
        }

        protected Task<CommandResult> Create<TCommand>(TCommand command, Action<TCommand> handler) where TCommand : class, IAggregateCommand
        {
            Guard.NotNull(handler, nameof(handler));

            return InvokeAsync(command, x =>
            {
                handler(x);

                return Task.FromResult<object?>(None.Value);
            }, Mode.Create);
        }

        protected Task<CommandResult> UpdateReturnAsync<TCommand>(TCommand command, Func<TCommand, Task<object?>> handler) where TCommand : class, IAggregateCommand
        {
            Guard.NotNull(handler, nameof(handler));

            return InvokeAsync(command, handler, Mode.Update);
        }

        protected Task<CommandResult> UpdateReturn<TCommand>(TCommand command, Func<TCommand, object?> handler) where TCommand : class, IAggregateCommand
        {
            Guard.NotNull(handler, nameof(handler));

            return InvokeAsync(command, x =>
            {
                var result = handler(x);

                return Task.FromResult(result);
            }, Mode.Update);
        }

        protected Task<CommandResult> UpdateAsync<TCommand>(TCommand command, Func<TCommand, Task> handler) where TCommand : class, IAggregateCommand
        {
            Guard.NotNull(handler, nameof(handler));

            return InvokeAsync(command, async x =>
            {
                await handler(x);

                return None.Value;
            }, Mode.Update);
        }

        protected Task<CommandResult> Update<TCommand>(TCommand command, Action<TCommand> handler) where TCommand : class, IAggregateCommand
        {
            Guard.NotNull(handler, nameof(handler));

            return InvokeAsync(command, x =>
            {
                handler(x);

                return Task.FromResult<object?>(None.Value);
            }, Mode.Update);
        }

        protected Task<CommandResult> UpsertReturnAsync<TCommand>(TCommand command, Func<TCommand, Task<object?>> handler) where TCommand : class, IAggregateCommand
        {
            Guard.NotNull(handler, nameof(handler));

            return InvokeAsync(command, handler, Mode.Upsert);
        }

        protected Task<CommandResult> UpsertReturn<TCommand>(TCommand command, Func<TCommand, object?> handler) where TCommand : class, IAggregateCommand
        {
            Guard.NotNull(handler, nameof(handler));

            return InvokeAsync(command, x =>
            {
                var result = handler(x);

                return Task.FromResult(result);
            }, Mode.Upsert);
        }

        protected Task<CommandResult> UpsertAsync<TCommand>(TCommand command, Func<TCommand, Task> handler) where TCommand : class, IAggregateCommand
        {
            Guard.NotNull(handler, nameof(handler));

            return InvokeAsync(command, async x =>
            {
                await handler(x);

                return None.Value;
            }, Mode.Upsert);
        }

        protected Task<CommandResult> Upsert<TCommand>(TCommand command, Action<TCommand> handler) where TCommand : class, IAggregateCommand
        {
            Guard.NotNull(handler, nameof(handler));

            return InvokeAsync(command, x =>
            {
                handler(x);

                return Task.FromResult<object?>(None.Value);
            }, Mode.Upsert);
        }

        private async Task<CommandResult> InvokeAsync<TCommand>(TCommand command, Func<TCommand, Task<object?>> handler, Mode mode) where TCommand : class, IAggregateCommand
        {
            Guard.NotNull(command, nameof(command));

            if (mode != Mode.Create)
            {
                await EnsureLoadedAsync();

                if (Version < 0 && mode == Mode.Update)
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

                if (Version < 0)
                {
                    if (!CanAcceptCreation(command))
                    {
                        throw new DomainException("Invalid command.");
                    }
                }
                else
                {
                    if (!CanAccept(command))
                    {
                        throw new DomainException("Invalid command.");
                    }
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
                var result = (await handler(command)) ?? None.Value;

                var events = uncomittedEvents.ToArray();

                await WriteAsync(events, previousVersion);

                isLoaded = true;

                return new CommandResult(UniqueId, Version, previousVersion, result);
            }
            catch (InconsistentStateException) when (mode == Mode.Create)
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

        public abstract Task<CommandResult> ExecuteAsync(IAggregateCommand command);
    }
}