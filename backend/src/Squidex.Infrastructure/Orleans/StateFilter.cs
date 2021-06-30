// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Orleans;
using Orleans.Runtime;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.States;

namespace Squidex.Infrastructure.Orleans
{
    public sealed class StateFilter : IIncomingGrainCallFilter
    {
        private readonly IGrainRuntime runtime;

        public StateFilter(IGrainRuntime runtime)
        {
            this.runtime = runtime;
        }

        public async Task Invoke(IIncomingGrainCallContext context)
        {
            try
            {
                await context.Invoke();
            }
            catch (DomainObjectNotFoundException)
            {
                TryDeactivate(context);

                throw;
            }
            catch (WrongEventVersionException)
            {
                TryDeactivate(context);

                throw;
            }
            catch (InconsistentStateException)
            {
                TryDeactivate(context);

                throw;
            }
        }

        private void TryDeactivate(IIncomingGrainCallContext context)
        {
            if (context.Grain is Grain grain)
            {
                runtime.DeactivateOnIdle(grain);
            }
        }
    }
}
