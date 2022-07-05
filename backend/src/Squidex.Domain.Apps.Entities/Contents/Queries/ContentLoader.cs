// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Orleans;
using Squidex.Domain.Apps.Entities.Contents.DomainObject;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.Queries
{
    public sealed class ContentLoader : IContentLoader
    {
        private readonly IGrainFactory grainFactory;

        public ContentLoader(IGrainFactory grainFactory)
        {
            this.grainFactory = grainFactory;
        }

        public async Task<IContentEntity?> GetAsync(DomainId appId, DomainId id, long version = EtagVersion.Any)
        {
            using (Telemetry.Activities.StartActivity("ContentLoader/GetAsync"))
            {
                var key = DomainId.Combine(appId, id).ToString();

                var contentGrain = grainFactory.GetGrain<IContentGrain>(key);
                var contentState = await contentGrain.GetStateAsync(version);

                if (contentState == null || contentState.Version <= EtagVersion.Empty || (version > EtagVersion.Any && contentState.Version != version))
                {
                    return null;
                }

                return contentState;
            }
        }
    }
}
