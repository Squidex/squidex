// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Contents.DomainObject;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities.Contents.Queries;

public sealed class ContentLoader : IContentLoader
{
    private readonly IDomainObjectFactory domainObjectFactory;
    private readonly IDomainObjectCache domainObjectCache;

    public ContentLoader(IDomainObjectFactory domainObjectFactory, IDomainObjectCache domainObjectCache)
    {
        this.domainObjectFactory = domainObjectFactory;
        this.domainObjectCache = domainObjectCache;
    }

    public async Task<IContentEntity?> GetAsync(DomainId appId, DomainId id, long version = EtagVersion.Any,
        CancellationToken ct = default)
    {
        var uniqueId = DomainId.Combine(appId, id);

        var content = await GetCachedAsync(uniqueId, version, ct);

        if (content == null)
        {
            content = await GetAsync(uniqueId, version, ct);
        }

        if (content is not { Version: > EtagVersion.Empty } || (version > EtagVersion.Any && content.Version != version))
        {
            return null;
        }

        return content;
    }

    private async Task<ContentDomainObject.State?> GetCachedAsync(DomainId uniqueId, long version,
        CancellationToken ct)
    {
        using (Telemetry.Activities.StartActivity("ContentLoader/GetCachedAsync"))
        {
            return await domainObjectCache.GetAsync<ContentDomainObject.State>(uniqueId, version, ct);
        }
    }

    private async Task<ContentDomainObject.State> GetAsync(DomainId uniqueId, long version,
        CancellationToken ct)
    {
        using (Telemetry.Activities.StartActivity("ContentLoader/GetAsync"))
        {
            var contentObject = domainObjectFactory.Create<ContentDomainObject>(uniqueId);

            return await contentObject.GetSnapshotAsync(version, ct);
        }
    }
}
