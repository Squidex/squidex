// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Orleans;
using Squidex.Infrastructure.States;
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
            var name = $"Grain/{context.Grain?.GetType().Name}/{context.ImplementationMethod?.Name}";

            using (Telemetry.Activities.StartActivity(name))
            {
                try
                {
                    await context.Invoke();
                }
                catch (DomainException ex)
                {
                    if (ex.InnerException != null)
                    {
                        Log(context, ex.InnerException);
                    }

                    throw;
                }
                catch (Exception ex)
                {
                    Log(context, ex);
                    throw;
                }
            }
        }

        private void Log(IIncomingGrainCallContext context, Exception ex)
        {
            log.LogError(ex, w => w
                .WriteProperty("action", "GrainInvoked")
                .WriteProperty("status", "Failed")
                .WriteProperty("grain", context.Grain.ToString())
                .WriteProperty("grainMethod", context.ImplementationMethod.ToString()));
        }
    }
}
