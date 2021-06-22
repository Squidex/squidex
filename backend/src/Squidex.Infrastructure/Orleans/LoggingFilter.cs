// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Orleans;
using Squidex.Log;

namespace Squidex.Infrastructure.Orleans
{
    public sealed class LoggingFilter : IIncomingGrainCallFilter
    {
        private readonly ISemanticLog log;

        public LoggingFilter(ISemanticLog log)
        {
            this.log = log;
        }

        public async Task Invoke(IIncomingGrainCallContext context)
        {
            try
            {
                await context.Invoke();
            }
            catch (DomainException)
            {
                throw;
            }
            catch (Exception ex)
            {
                log.LogError(ex, w => w
                    .WriteProperty("action", "GrainInvoked")
                    .WriteProperty("status", "Failed")
                    .WriteProperty("grain", context.Grain.ToString())
                    .WriteProperty("grainMethod", context.ImplementationMethod.ToString()));

                throw;
            }
        }
    }
}
