// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Orleans.Runtime;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.States;

namespace Squidex.Infrastructure.Orleans
{
    public sealed class GrainState<T> : IGrainState<T> where T : class, new()
    {
        private readonly IGrainActivationContext context;
        private IPersistence<T> persistence;

        public T Value { get; set; } = new T();

        public long Version
        {
            get => persistence.Version;
        }

        public GrainState(IGrainActivationContext context)
        {
            Guard.NotNull(context, nameof(context));

            this.context = context;

            context.ObservableLifecycle.Subscribe("Persistence", GrainLifecycleStage.SetupState, SetupAsync);
        }

        public Task SetupAsync(
            CancellationToken ct = default)
        {
            if (ct.IsCancellationRequested)
            {
                return Task.CompletedTask;
            }

            DomainId key;

            if (context.GrainIdentity.PrimaryKeyString != null)
            {
                key = DomainId.Create(context.GrainIdentity.PrimaryKeyString);
            }
            else
            {
                key = DomainId.Create(context.GrainIdentity.PrimaryKey);
            }

            var factory = context.ActivationServices.GetRequiredService<IPersistenceFactory<T>>();

            persistence = factory.WithSnapshots(GetType(), key, ApplyState);

            return persistence.ReadAsync();
        }

        private void ApplyState(T value, long version)
        {
            Value = value;
        }

        public Task ClearAsync()
        {
            Value = new T();

            return persistence.DeleteAsync();
        }

        public Task WriteAsync()
        {
            return persistence.WriteSnapshotAsync(Value);
        }

        public Task WriteEventAsync(Envelope<IEvent> envelope)
        {
            return persistence.WriteEventAsync(envelope);
        }
    }
}
