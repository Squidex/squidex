// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Tags
{
    public sealed class GrainTagService : ITagService
    {
        private readonly IGrainFactory grainFactory;

        public string Name
        {
            get { return "Tags"; }
        }

        public GrainTagService(IGrainFactory grainFactory)
        {
            Guard.NotNull(grainFactory, nameof(grainFactory));

            this.grainFactory = grainFactory;
        }

        public Task<HashSet<string>> NormalizeTagsAsync(Guid appId, string group, HashSet<string> names, HashSet<string> ids)
        {
            return GetGrain(appId, group).NormalizeTagsAsync(names, ids);
        }

        public Task<HashSet<string>> GetTagIdsAsync(Guid appId, string group, HashSet<string> names)
        {
            return GetGrain(appId, group).GetTagIdsAsync(names);
        }

        public Task<Dictionary<string, string>> DenormalizeTagsAsync(Guid appId, string group, HashSet<string> ids)
        {
            return GetGrain(appId, group).DenormalizeTagsAsync(ids);
        }

        public Task<Dictionary<string, int>> GetTagsAsync(Guid appId, string group)
        {
            return GetGrain(appId, group).GetTagsAsync();
        }

        public Task RebuildTagsAsync(Guid appId, string group, Dictionary<string, string> allTags)
        {
            return GetGrain(appId, group).RebuildTagsAsync(allTags);
        }

        public Task ClearAsync(Guid appId, string group)
        {
            return GetGrain(appId, group).ClearAsync();
        }

        private ITagGrain GetGrain(Guid appId, string group)
        {
            Guard.NotNullOrEmpty(group, nameof(group));

            return grainFactory.GetGrain<ITagGrain>($"{appId}_{group}");
        }
    }
}
