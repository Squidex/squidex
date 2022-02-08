﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Logging;
using Orleans;
using Squidex.Log;

namespace Squidex.Infrastructure.Orleans
{
    public sealed class LoggingFilter : IIncomingGrainCallFilter
    {
        private readonly ILoggerFactory logFactory;

        public LoggingFilter(ILoggerFactory logFactory)
        {
            this.logFactory = logFactory;
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
            var log = logFactory.CreateLogger(context.Grain.GetType());

            log.LogError(ex, "Failed to execute method of grain.", context.ImplementationMethod);
        }
    }
}
