// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Orleans;

namespace Squidex.Infrastructure.Orleans
{
    public sealed class ActivationLimiterFilter : IIncomingGrainCallFilter
    {
        public Task Invoke(IIncomingGrainCallContext context)
        {
            if (context.Grain is GrainBase grainBase)
            {
                grainBase.ReportIAmAlive();
            }

            return context.Invoke();
        }
    }
}
