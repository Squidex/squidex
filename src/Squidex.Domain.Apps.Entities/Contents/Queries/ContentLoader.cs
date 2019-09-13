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

namespace Squidex.Domain.Apps.Entities.Contents.Queries
{
    public sealed class ContentLoader : IContentLoader
    {
        private readonly IGrainFactory grainFactory;

        public ContentLoader(IGrainFactory grainFactory)
        {
            Guard.NotNull(grainFactory, nameof(grainFactory));

            this.grainFactory = grainFactory;
        }

        public async Task<IContentEntity> GetAsync(Guid id, long version)
        {
            using (Profiler.TraceMethod<ContentLoader>())
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
