// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.DependencyInjection;
using Squidex.Domain.Apps.Entities.Contents.DomainObject;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.Queries
{
    public sealed class ContentLoader : IContentLoader
    {
        private readonly Func<DomainId, ContentDomainObject> factory;

        public ContentLoader(IServiceProvider serviceProvider)
        {
            var objectFactory = ActivatorUtilities.CreateFactory(typeof(ContentDomainObject), new[] { typeof(DomainId) });

            factory = key =>
            {
                return (ContentDomainObject)objectFactory(serviceProvider, new object[] { key });
            };
        }

        public async Task<IContentEntity?> GetAsync(DomainId appId, DomainId id, long version = EtagVersion.Any)
        {
            using (Telemetry.Activities.StartActivity("ContentLoader/GetAsync"))
            {
                var key = DomainId.Combine(appId, id);

                var contentObject = factory(key);
                var contentState = await contentObject.GetSnapshotAsync(version);

                if (contentState == null || contentState.Version <= EtagVersion.Empty || (version > EtagVersion.Any && contentState.Version != version))
                {
                    return null;
                }

                return contentState;
            }
        }
    }
}
