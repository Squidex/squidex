﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Infrastructure.States;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Infrastructure.Commands
{
    public sealed class AggregateHandler : IAggregateHandler
    {
        private readonly AsyncLockPool lockPool = new AsyncLockPool(10000);
        private readonly IStateFactory stateFactory;
        private readonly IServiceProvider serviceProvider;

        public AggregateHandler(IStateFactory stateFactory, IServiceProvider serviceProvider)
        {
            Guard.NotNull(stateFactory, nameof(stateFactory));
            Guard.NotNull(serviceProvider, nameof(serviceProvider));

            this.stateFactory = stateFactory;
            this.serviceProvider = serviceProvider;
        }

        public Task<T> CreateAsync<T>(CommandContext context, Func<T, Task> creator) where T : class, IDomainObject
        {
            Guard.NotNull(creator, nameof(creator));

            return InvokeAsync(context, creator, false);
        }

        public Task<T> UpdateAsync<T>(CommandContext context, Func<T, Task> updater) where T : class, IDomainObject
        {
            Guard.NotNull(updater, nameof(updater));

            return InvokeAsync(context, updater, true);
        }

        public Task<T> CreateSyncedAsync<T>(CommandContext context, Func<T, Task> creator) where T : class, IDomainObject
        {
            Guard.NotNull(creator, nameof(creator));

            return InvokeSyncedAsync(context, creator, false);
        }

        public Task<T> UpdateSyncedAsync<T>(CommandContext context, Func<T, Task> updater) where T : class, IDomainObject
        {
            Guard.NotNull(updater, nameof(updater));

            return InvokeSyncedAsync(context, updater, true);
        }

        private async Task<T> InvokeAsync<T>(CommandContext context, Func<T, Task> handler, bool isUpdate) where T : class, IDomainObject
        {
            Guard.NotNull(context, nameof(context));

            var domainCommand = GetCommand(context);
            var domainObjectId = domainCommand.AggregateId;
            var domainObject = await stateFactory.CreateAsync<T>(domainObjectId);

            if (domainCommand.ExpectedVersion != EtagVersion.Any && domainCommand.ExpectedVersion != domainObject.Version)
            {
                throw new DomainObjectVersionException(domainObjectId.ToString(), typeof(T), domainObject.Version, domainCommand.ExpectedVersion);
            }

            await handler(domainObject);

            await domainObject.WriteAsync();

            if (!context.IsCompleted)
            {
                if (isUpdate)
                {
                    context.Complete(new EntitySavedResult(domainObject.Version));
                }
                else
                {
                    context.Complete(EntityCreatedResult.Create(domainObjectId, domainObject.Version));
                }
            }

            return domainObject;
        }

        private async Task<T> InvokeSyncedAsync<T>(CommandContext context, Func<T, Task> handler, bool isUpdate) where T : class, IDomainObject
        {
            Guard.NotNull(context, nameof(context));

            var domainCommand = GetCommand(context);
            var domainObjectId = domainCommand.AggregateId;

            using (await lockPool.LockAsync(Tuple.Create(typeof(T), domainObjectId)))
            {
                var domainObject = await stateFactory.GetSingleAsync<T>(domainObjectId);

                if (domainCommand.ExpectedVersion != EtagVersion.Any && domainCommand.ExpectedVersion != domainObject.Version)
                {
                    throw new DomainObjectVersionException(domainObjectId.ToString(), typeof(T), domainObject.Version, domainCommand.ExpectedVersion);
                }

                await handler(domainObject);

                try
                {
                    await domainObject.WriteAsync();

                    stateFactory.Synchronize<T, Guid>(domainObjectId);
                }
                catch
                {
                    stateFactory.Remove<T, Guid>(domainObjectId);

                    throw;
                }

                if (!context.IsCompleted)
                {
                    if (isUpdate)
                    {
                        context.Complete(new EntitySavedResult(domainObject.Version));
                    }
                    else
                    {
                        context.Complete(EntityCreatedResult.Create(domainObjectId, domainObject.Version));
                    }
                }

                return domainObject;
            }
        }

        private static IAggregateCommand GetCommand(CommandContext context)
        {
            if (!(context.Command is IAggregateCommand command))
            {
                throw new ArgumentException("Context must have an aggregate command.", nameof(context));
            }

            Guard.NotEmpty(command.AggregateId, "context.Command.AggregateId");

            return command;
        }
    }
}
