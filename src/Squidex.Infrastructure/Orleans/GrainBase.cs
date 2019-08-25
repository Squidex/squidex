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
using Squidex.Infrastructure.Tasks;

namespace Squidex.Infrastructure.Orleans
{
    public abstract class GrainBase : Grain, IDeactivatableGrain
    {
        private IGrainLimiterServiceClient grainLimiterServiceClient;

        public virtual (int MaxActivations, Type Interface) Limitations { get => (-1, null); }

        public Task DeactivateAsync()
        {
            DeactivateOnIdle();

            return Task.CompletedTask;
        }

        public override void Participate(IGrainLifecycle lifecycle)
        {
            if (Limitations.MaxActivations > 0 && Limitations.Interface != null)
            {
                grainLimiterServiceClient = ServiceProvider.GetRequiredService<IGrainLimiterServiceClient>();

                lifecycle.Subscribe("Limiter", GrainLifecycleStage.Activate, RegisterAsync, UnregisterAsync);
            }

            base.Participate(lifecycle);
        }

        public void ReportIAmAlive()
        {
            RegisterAsync().Forget();
        }

        private async Task RegisterAsync(CancellationToken ct = default)
        {
            if (grainLimiterServiceClient == null || this.IsPrimaryKeyBasedOnLong())
            {
                return;
            }

            var keyAsString = this.GetPrimaryKeyString();

            if (keyAsString != null)
            {
                await grainLimiterServiceClient.RegisterAsync(Limitations.Interface, keyAsString, Limitations.MaxActivations);
            }
            else
            {
                await grainLimiterServiceClient.RegisterAsync(Limitations.Interface, this.GetPrimaryKey(), Limitations.MaxActivations);
            }
        }

        private async Task UnregisterAsync(CancellationToken ct = default)
        {
            if (grainLimiterServiceClient == null || this.IsPrimaryKeyBasedOnLong())
            {
                return;
            }

            var keyAsString = this.GetPrimaryKeyString();

            if (keyAsString != null)
            {
                await grainLimiterServiceClient.UnregisterAsync(Limitations.Interface, keyAsString);
            }
            else
            {
                await grainLimiterServiceClient.UnregisterAsync(Limitations.Interface, this.GetPrimaryKey());
            }
        }
    }
}
