// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Orleans.Runtime;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Infrastructure.Orleans
{
    public sealed class ActivationLimit : IActivationLimit, IDeactivater
    {
        private readonly IGrainActivationContext context;
        private readonly IActivationLimiter limiter;
        private Action register;
        private Action unregister;

        public ActivationLimit(IGrainActivationContext context, IActivationLimiter limiter)
        {
            this.context = context;
            this.limiter = limiter;

            context.ObservableLifecycle?.Subscribe("Limiter", GrainLifecycleStage.Activate,
            ct =>
            {
                ReportIAmAlive();

                return TaskHelper.Done;
            },
            ct =>
            {
                ReportIAmDead();

                return TaskHelper.Done;
            });
        }

        public void ReportIAmAlive()
        {
            register?.Invoke();
        }

        public void ReportIAmDead()
        {
            unregister?.Invoke();
        }

        public void SetLimit(int maxActivations, TimeSpan lifetime)
        {
            register = () =>
            {
                limiter.Register(context.GrainType, this, maxActivations);
            };

            unregister = () =>
            {
                limiter.Unregister(context.GrainType, this);
            };

            var runtime = context.ActivationServices.GetRequiredService<IGrainRuntime>();

            runtime.DelayDeactivation(context.GrainInstance, lifetime);
        }

        void IDeactivater.Deactivate()
        {
            var runtime = context.ActivationServices.GetRequiredService<IGrainRuntime>();

            runtime.DeactivateOnIdle(context.GrainInstance);
        }
    }
}