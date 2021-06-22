// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;
using Squidex.Domain.Apps.Core.Tags;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Tags
{
    public sealed class GrainTagService : ITagService
    {
        private readonly IGrainFactory grainFactory;

        public GrainTagService(IGrainFactory grainFactory)
        {
            this.grainFactory = grainFactory;
        }

        public Task<Dictionary<string, string>> NormalizeTagsAsync(DomainId appId, string group, HashSet<string>? names, HashSet<string>? ids)
        {
            return GetGrain(appId, group).NormalizeTagsAsync(names, ids);
        }

        public Task<Dictionary<string, string>> GetTagIdsAsync(DomainId appId, string group, HashSet<string> names)
        {
            return GetGrain(appId, group).GetTagIdsAsync(names);
        }

        public Task<Dictionary<string, string>> DenormalizeTagsAsync(DomainId appId, string group, HashSet<string> ids)
        {
            return GetGrain(appId, group).DenormalizeTagsAsync(ids);
        }

        public Task<TagsSet> GetTagsAsync(DomainId appId, string group)
        {
            return GetGrain(appId, group).GetTagsAsync();
        }

        public Task<TagsExport> GetExportableTagsAsync(DomainId appId, string group)
        {
            return GetGrain(appId, group).GetExportableTagsAsync();
        }

        public Task RebuildTagsAsync(DomainId appId, string group, TagsExport tags)
        {
            return GetGrain(appId, group).RebuildAsync(tags);
        }

        public Task ClearAsync(DomainId appId, string group)
        {
            return GetGrain(appId, group).ClearAsync();
        }

        private ITagGrain GetGrain(DomainId appId, string group)
        {
            Guard.NotNullOrEmpty(group, nameof(group));

            return grainFactory.GetGrain<ITagGrain>($"{appId}_{group}");
        }
    }
}
