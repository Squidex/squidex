// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Orleans;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Log;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public sealed class ContentVersionLoader : IContentVersionLoader
    {
        private readonly IGrainFactory grainFactory;

        public ContentVersionLoader(IGrainFactory grainFactory)
        {
            Guard.NotNull(grainFactory, nameof(grainFactory));

            this.grainFactory = grainFactory;
        }

        public async Task<IContentEntity> LoadAsync(Guid id, long version)
        {
            using (Profiler.TraceMethod<ContentVersionLoader>())
            {
                var grain = grainFactory.GetGrain<IContentGrain>(id);

                var content = await grain.GetStateAsync(version);

                if (content.Value == null || content.Value.Version != version)
                {
                    throw new DomainObjectNotFoundException(id.ToString(), typeof(IContentEntity));
                }

                return content.Value;
            }
        }
    }
}
