// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Concurrent;

namespace Squidex.Infrastructure.Log
{
    public sealed class ProfilerSession
    {
        private struct ProfilerItem
        {
            public long Total;
            public long Count;
        }

        private readonly ConcurrentDictionary<string, ProfilerItem> traces = new ConcurrentDictionary<string, ProfilerItem>();

        public void Measured(string name, long elapsed)
        {
            Guard.NotNullOrEmpty(name, nameof(name));

            traces.AddOrUpdate(name, x =>
            {
                return new ProfilerItem { Total = elapsed, Count = 1 };
            },
            (x, result) =>
            {
                result.Total += elapsed;
                result.Count++;

                return result;
            });
        }

        public void Write(IObjectWriter writer)
        {
            Guard.NotNull(writer, nameof(writer));

            if (traces.Count > 0)
            {
                writer.WriteObject("profiler", p =>
                {
                    foreach (var kvp in traces)
                    {
                        p.WriteObject(kvp.Key, k => k
                            .WriteProperty("elapsedMsTotal", kvp.Value.Total)
                            .WriteProperty("elapsedMsAvg", kvp.Value.Total / kvp.Value.Count)
                            .WriteProperty("count", kvp.Value.Count));
                    }
                });
            }
        }
    }
}
