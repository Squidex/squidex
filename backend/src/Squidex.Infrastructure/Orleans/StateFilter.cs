// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Orleans;
using Orleans.Storage;
using StateInconsistentStateException = Squidex.Infrastructure.States.InconsistentStateException;

namespace Squidex.Infrastructure.Orleans
{
    public sealed class StateFilter : IIncomingGrainCallFilter
    {
        public async Task Invoke(IIncomingGrainCallContext context)
        {
            try
            {
                await context.Invoke();
            }
            catch (StateInconsistentStateException ex)
            {
                throw new InconsistentStateException(ex.Message, ex);
            }
        }
    }
}
