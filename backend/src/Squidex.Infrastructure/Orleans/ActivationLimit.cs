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
using Orleans.Runtime;

namespace Squidex.Infrastructure.Orleans
{
    public sealed class ActivationLimit : IActivationLimit, IDeactivater
    {
        private readonly IGrainActivationContext context;
        private readonly IActivationLimiter limiter;
        private int maxActivations;

        public ActivationLimit(IGrainActivationContext context, IActivationLimiter limiter)
        {
            this.context = context;
            this.limiter = limiter;
        }

        public void ReportIAmAlive()
        {
            if (maxActivations > 0)
            {
                limiter.Register(context.GrainType, this, maxActivations);
            }
        }

        public void ReportIAmDead()
        {
            if (maxActivations > 0)
            {
                limiter.Unregister(context.GrainType, this);
            }
        }

        public void SetLimit(int activations, TimeSpan lifetime)
        {
            maxActivations = activations;

            context.ObservableLifecycle?.Subscribe("Limiter", GrainLifecycleStage.Activate,
                ct =>
                {
                    var runtime = context.ActivationServices.GetRequiredService<IGrainRuntime>();

                    runtime.DelayDeactivation(context.GrainInstance, lifetime);

                    ReportIAmAlive();

                    return Task.CompletedTask;
                },
                ct =>
                {
                    ReportIAmDead();

                    return Task.CompletedTask;
                });
        }

        void IDeactivater.Deactivate()
        {
            var runtime = context.ActivationServices.GetRequiredService<IGrainRuntime>();

            runtime.DeactivateOnIdle(context.GrainInstance);
        }
    }
}