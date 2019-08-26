// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Orleans.Core;
using Orleans.Runtime;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Infrastructure.Orleans
{
    public abstract class GrainBase : Grain, IDeactivatableGrain
    {
        private IActivationLimiter grainLimiter;

        public virtual IActivationLimit Limit { get; }

        protected GrainBase()
        {
        }

        protected GrainBase(IGrainIdentity identity, IGrainRuntime runtime)
            : base(identity, runtime)
        {
        }

        public Task DeactivateAsync()
        {
            DeactivateOnIdle();

            return Task.CompletedTask;
        }

        public override void Participate(IGrainLifecycle lifecycle)
        {
            if (Limit != null)
            {
                grainLimiter = ServiceProvider.GetRequiredService<IActivationLimiter>();

                lifecycle?.Subscribe("Limiter", GrainLifecycleStage.Activate,
                    ct =>
                    {
                        ReportIAmAlive();

                        return TaskHelper.Done;
                    },
                    ct =>
                    {
                        ReportIamDead();

                        return TaskHelper.Done;
                    });
            }

            if (lifecycle != null)
            {
                base.Participate(lifecycle);
            }
        }

        public void ReportIAmAlive()
        {
            Limit?.Register(grainLimiter, this);
        }

        public void ReportIamDead()
        {
            Limit?.Unregister(grainLimiter, this);
        }
    }
}
