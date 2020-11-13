// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Orleans;
using Squidex.Infrastructure;
using Squidex.Log;

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

        public async Task<IContentEntity> GetAsync(DomainId appId, DomainId id, long version)
        {
            using (Profiler.TraceMethod<ContentLoader>())
            {
                var key = DomainId.Combine(appId, id).ToString();

                var contentGrain = grainFactory.GetGrain<IContentGrain>(key);
                var contentState = await contentGrain.GetStateAsync(version);

                var content = contentState.Value;

                if (content == null || content.Version <= EtagVersion.Empty || (version > EtagVersion.Any && content.Version != version))
                {
                    throw new DomainObjectNotFoundException(id.ToString());
                }

                return content;
            }
        }
    }
}
