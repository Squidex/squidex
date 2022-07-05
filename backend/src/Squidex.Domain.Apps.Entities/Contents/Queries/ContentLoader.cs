// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.DependencyInjection;
using Squidex.Domain.Apps.Entities.Contents.DomainObject;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities.Contents.Queries
{
    public sealed class ContentLoader : IContentLoader
    {
        private readonly IDomainObjectFactory domainObjectFactory;

        public ContentLoader(IDomainObjectFactory domainObjectFactory)
        {
            this.domainObjectFactory = domainObjectFactory;
        }

        public async Task<IContentEntity?> GetAsync(DomainId appId, DomainId id, long version = EtagVersion.Any,
            CancellationToken ct = default)
        {
            using (Telemetry.Activities.StartActivity("ContentLoader/GetAsync"))
            {
                var key = DomainId.Combine(appId, id);

                var contentObject = domainObjectFactory.Create<ContentDomainObject>(key);
                var contentState = await contentObject.GetSnapshotAsync(version, ct);

                if (contentState == null || contentState.Version <= EtagVersion.Empty || (version > EtagVersion.Any && contentState.Version != version))
                {
                    return null;
                }

                return contentState;
            }
        }
    }
}
