// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.Queries.Steps
{
    public sealed class EnrichWithWorkflows : IContentEnricherStep
    {
        private const string DefaultColor = StatusColors.Draft;

        private readonly IContentWorkflow contentWorkflow;

        public EnrichWithWorkflows(IContentWorkflow contentWorkflow)
        {
            Guard.NotNull(contentWorkflow);

            this.contentWorkflow = contentWorkflow;
        }

        public async Task EnrichAsync(Context context, IEnumerable<ContentEntity> contents, ProvideSchema schemas)
        {
            var cache = new Dictionary<(Guid, Status), StatusInfo>();

            foreach (var content in contents)
            {
                await EnrichColorAsync(content, content, cache);

                if (ShouldEnrichWithStatuses(context))
                {
                    await EnrichNextsAsync(content, context);
                    await EnrichCanUpdateAsync(content, context);
                }
            }
        }

        private async Task EnrichNextsAsync(ContentEntity content, Context context)
        {
            content.Nexts = await contentWorkflow.GetNextsAsync(content, context.User);
        }

        private async Task EnrichCanUpdateAsync( ContentEntity content, Context context)
        {
            content.CanUpdate = await contentWorkflow.CanUpdateAsync(content, context.User);
        }

        private async Task EnrichColorAsync(IContentEntity content, ContentEntity result, Dictionary<(Guid, Status), StatusInfo> cache)
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

        private static bool ShouldEnrichWithStatuses(Context context)
        {
            return context.IsFrontendClient || context.ShouldResolveFlow();
        }
    }
}
