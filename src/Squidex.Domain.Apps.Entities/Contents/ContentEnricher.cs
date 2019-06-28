// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
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
        private readonly IContextProvider contextProvider;

        public ContentEnricher(IContentWorkflow contentWorkflow, IContextProvider contextProvider)
        {
            Guard.NotNull(contentWorkflow, nameof(contentWorkflow));
            Guard.NotNull(contextProvider, nameof(contextProvider));

            this.contentWorkflow = contentWorkflow;
            this.contextProvider = contextProvider;
        }

        public async Task<IEnrichedContentEntity> EnrichAsync(IContentEntity content, ClaimsPrincipal user)
        {
            Guard.NotNull(content, nameof(content));

            var enriched = await EnrichAsync(Enumerable.Repeat(content, 1), user);

            return enriched[0];
        }

        public async Task<IReadOnlyList<IEnrichedContentEntity>> EnrichAsync(IEnumerable<IContentEntity> contents, ClaimsPrincipal user)
        {
            Guard.NotNull(contents, nameof(contents));
            Guard.NotNull(user, nameof(user));

            using (Profiler.TraceMethod<ContentEnricher>())
            {
                var results = new List<ContentEntity>();

                var cache = new Dictionary<(Guid, Status), StatusInfo>();

                foreach (var content in contents)
                {
                    var result = SimpleMapper.Map(content, new ContentEntity());

                    await ResolveColorAsync(content, result, cache);

                    if (ShouldEnrichWithStatuses())
                    {
                        await ResolveNextsAsync(content, result, user);
                        await ResolveCanUpdateAsync(content, result);
                    }

                    results.Add(result);
                }

                return results;
            }
        }

        private bool ShouldEnrichWithStatuses()
        {
            return contextProvider.Context.IsFrontendClient || contextProvider.Context.IsResolveFlow();
        }

        private async Task ResolveCanUpdateAsync(IContentEntity content, ContentEntity result)
        {
            result.CanUpdate = await contentWorkflow.CanUpdateAsync(content);
        }

        private async Task ResolveNextsAsync(IContentEntity content, ContentEntity result, ClaimsPrincipal user)
        {
            result.Nexts = await contentWorkflow.GetNextsAsync(content, user);
        }

        private async Task ResolveColorAsync(IContentEntity content, ContentEntity result, Dictionary<(Guid, Status), StatusInfo> cache)
        {
            result.StatusColor = await GetColorAsync(content, cache);
        }

        private async Task<string> GetColorAsync(IContentEntity content, Dictionary<(Guid, Status), StatusInfo> cache)
        {
            if (!cache.TryGetValue((content.SchemaId.Id, content.Status), out var info))
            {
                info = await contentWorkflow.GetInfoAsync(content);

                if (info == null)
                {
                    info = new StatusInfo(content.Status, DefaultColor);
                }

                cache[(content.SchemaId.Id, content.Status)] = info;
            }

            return info.Color;
        }
    }
}
