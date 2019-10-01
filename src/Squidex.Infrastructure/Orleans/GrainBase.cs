// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Orleans.Core;
using Orleans.Runtime;

namespace Squidex.Infrastructure.Orleans
{
    public abstract class GrainBase : Grain
    {
        protected GrainBase()
        {
        }

        protected GrainBase(IGrainIdentity? identity, IGrainRuntime? runtime)
            : base(identity, runtime)
        {
        }

        public void ReportIAmAlive()
        {
            var limit = ServiceProvider.GetService<IActivationLimit>();

            limit?.ReportIAmAlive();
        }

        public void ReportIAmDead()
        {
            var limit = ServiceProvider.GetService<IActivationLimit>();

            limit?.ReportIAmDead();
        }

        protected void TryDelayDeactivation(TimeSpan timeSpan)
        {
            try
            {
                DelayDeactivation(timeSpan);
            }
            catch (InvalidOperationException)
            {
            }
        }

        protected void TryDeactivateOnIdle()
        {
            try
            {
                DeactivateOnIdle();
            }
            catch (InvalidOperationException)
            {
            }
        }
    }
}
