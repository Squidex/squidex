// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public sealed class ContentEnricher : IContentEnricher
    {
        private const string DefaultColor = StatusColors.Draft;
        private readonly IContentWorkflow contentWorkflow;

        public ContentEnricher(IContentWorkflow contentWorkflow)
        {
            this.contentWorkflow = contentWorkflow;
        }

        public async Task<IContentEntityEnriched> EnrichAsync(IContentEntity content)
        {
            Guard.NotNull(content, nameof(content));

            var enriched = await EnrichAsync(Enumerable.Repeat(content, 1));

            return enriched[0];
        }

        public async Task<IReadOnlyList<IContentEntityEnriched>> EnrichAsync(IEnumerable<IContentEntity> contents)
        {
            Guard.NotNull(contents, nameof(contents));

            using (Profiler.TraceMethod<ContentEnricher>())
            {
                var results = new List<ContentEntity>();

                var cache = new Dictionary<(Guid, Status), StatusInfo>();

                foreach (var content in contents)
                {
                    var result = SimpleMapper.Map(content, new ContentEntity());

                    await ResolveColorAsync(content, result, cache);
                    await ResolveNextsAsync(content, result);
                    await ResolveCanUpdateAsync(content, result);

                    results.Add(result);
                }

                return results;
            }
        }

        private async Task ResolveCanUpdateAsync(IContentEntity content, ContentEntity result)
        {
            result.CanUpdate = await contentWorkflow.CanUpdateAsync(content);
        }

        private async Task ResolveNextsAsync(IContentEntity content, ContentEntity result)
        {
            result.Nexts = await contentWorkflow.GetNextsAsync(content);
        }

        private async Task ResolveColorAsync(IContentEntity content, ContentEntity result, Dictionary<(Guid, Status), StatusInfo> cache)
        {
            result.StatusColor = await GetColorAsync(content.SchemaId, content.Status, cache);
        }

        private async Task<string> GetColorAsync(NamedId<Guid> schemaId, Status status, Dictionary<(Guid, Status), StatusInfo> cache)
        {
            if (!cache.TryGetValue((schemaId.Id, status), out var info))
            {
                info = await contentWorkflow.GetInfoAsync(status);

                if (info == null)
                {
                    info = new StatusInfo(status, DefaultColor);
                }

                cache[(schemaId.Id, status)] = info;
            }

            return info.Color;
        }
    }
}
