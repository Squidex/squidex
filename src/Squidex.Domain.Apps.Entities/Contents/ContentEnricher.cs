// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public sealed class ContentEnricher : IContentEnricher
    {
        private readonly IContentWorkflow contentWorkflow;

        public ContentEnricher(IContentWorkflow contentWorkflow)
        {
            this.contentWorkflow = contentWorkflow;
        }

        public async Task<IReadOnlyList<IEnrichedContentEntity>> EnrichAsync(IEnumerable<IContentEntity> contents)
        {
            var results = new List<ContentEntity>();

            using (Profiler.TraceMethod<ContentEnricher>())
            {
                var cache = new Dictionary<Status, StatusInfo>();

                foreach (var content in contents)
                {
                    var result = SimpleMapper.Map(content, new ContentEntity());

                    if (!cache.TryGetValue(content.Status, out var info))
                    {
                        info = await contentWorkflow.GetInfoAsync(content.Status);

                        cache[content.Status] = info;
                    }

                    result.StatusInfo = info;
                    result.Nexts = await contentWorkflow.GetNextsAsync(content);

                    results.Add(result);
                }
            }

            return results;
        }
    }
}
