// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Orleans;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Entities.Contents.Counter
{
    public sealed class CounterJintExtension : IJintExtension
    {
        private readonly IGrainFactory grainFactory;

        public CounterJintExtension(IGrainFactory grainFactory)
        {
            this.grainFactory = grainFactory;
        }

        public void Extend(ExecutionContext context)
        {
            if (context.TryGetValue<DomainId>("appId", out var appId))
            {
                var engine = context.Engine;

                engine.SetValue("incrementCounter", new Func<string, long>(name =>
                {
                    return Increment(appId, name);
                }));

                engine.SetValue("resetCounter", new Func<string, long, long>((name, value) =>
                {
                    return Reset(appId, name, value);
                }));
            }
        }

        private long Increment(DomainId appId, string name)
        {
            var grain = grainFactory.GetGrain<ICounterGrain>(appId.ToString());

            return AsyncHelper.Sync(() => grain.IncrementAsync(name));
        }

        private long Reset(DomainId appId, string name, long value)
        {
            var grain = grainFactory.GetGrain<ICounterGrain>(appId.ToString());

            return AsyncHelper.Sync(() => grain.ResetAsync(name, value));
        }
    }
}
