// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Orleans.Core;
using Orleans.Runtime;

namespace Squidex.Infrastructure.Orleans
{
    public abstract class GrainBase : Grain
    {
        public DomainId Key { get; private set; }

        protected GrainBase(IGrainIdentity identity, IGrainRuntime? runtime = null)
            : base(identity, runtime)
        {
            Guard.NotNull(identity);

            Key = DomainId.Create(identity.PrimaryKeyString);
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
