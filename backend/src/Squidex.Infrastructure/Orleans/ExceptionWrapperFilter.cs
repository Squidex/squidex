// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Orleans;

namespace Squidex.Infrastructure.Orleans
{
    public sealed class ExceptionWrapperFilter : IIncomingGrainCallFilter
    {
        public async Task Invoke(IIncomingGrainCallContext context)
        {
            try
            {
                await context.Invoke();
            }
            catch (Exception ex)
            {
                var type = ex.GetType();

                if (!type.IsSerializable)
                {
                    throw new OrleansWrapperException(ex, type);
                }
                else
                {
                    throw;
                }
            }
        }
    }
}
