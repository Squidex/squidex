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

        public GrainTagService(IGrainFactory grainFactory)
        {
            Guard.NotNull(grainFactory, nameof(grainFactory));

            this.grainFactory = grainFactory;
        }

        public Task<HashSet<string>> NormalizeTagsAsync(Guid appId, string category, HashSet<string> names, HashSet<string> ids)
        {
            return GetGrain(appId, category).NormalizeTagsAsync(names, ids);
        }

        public Task<HashSet<string>> GetTagIdsAsync(Guid appId, string category, HashSet<string> names)
        {
            return GetGrain(appId, category).GetTagIdsAsync(names);
        }

        public Task<Dictionary<string, string>> DenormalizeTagsAsync(Guid appId, string category, HashSet<string> ids)
        {
            return GetGrain(appId, category).DenormalizeTagsAsync(ids);
        }

        public Task<Dictionary<string, int>> GetTagsAsync(Guid appId, string category)
        {
            return GetGrain(appId, category).GetTagsAsync();
        }

        private ITagGrain GetGrain(Guid appId, string category)
        {
            Guard.NotNullOrEmpty(category, nameof(category));

            return grainFactory.GetGrain<ITagGrain>($"{appId}_{category}");
        }
    }
}
