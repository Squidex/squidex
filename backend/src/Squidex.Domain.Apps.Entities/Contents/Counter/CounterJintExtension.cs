// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
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
            Guard.NotNull(grainFactory, nameof(grainFactory));

            this.grainFactory = grainFactory;
        }

        public void Extend(ExecutionContext context, bool async)
        {
            if (context.TryGetValue("appId", out var temp) && temp is Guid appId)
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

        private long Increment(Guid appId, string name)
        {
            var grain = grainFactory.GetGrain<ICounterGrain>(appId);

            return AsyncHelper.Sync(() => grain.IncrementAsync(name));
        }

        private long Reset(Guid appId, string name, long value)
        {
            var grain = grainFactory.GetGrain<ICounterGrain>(appId);

            return AsyncHelper.Sync(() => grain.ResetAsync(name, value));
        }
    }
}
