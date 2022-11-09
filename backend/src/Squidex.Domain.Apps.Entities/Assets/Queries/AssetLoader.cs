// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Assets.DomainObject;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities.Assets.Queries;

public sealed class AssetLoader : IAssetLoader
{
    private readonly IDomainObjectFactory domainObjectFactory;
    private readonly IDomainObjectCache domainObjectCache;

    public AssetLoader(IDomainObjectFactory domainObjectFactory, IDomainObjectCache domainObjectCache)
    {
        this.domainObjectFactory = domainObjectFactory;
        this.domainObjectCache = domainObjectCache;
    }

    public async Task<IAssetEntity?> GetAsync(DomainId appId, DomainId id, long version = EtagVersion.Any,
        CancellationToken ct = default)
    {
        var uniqueId = DomainId.Combine(appId, id);

        var asset = await GetCachedAsync(uniqueId, version, ct);

        if (asset == null)
        {
            asset = await GetAsync(uniqueId, version, ct);
        }

        if (asset is not { Version: > EtagVersion.Empty } || (version > EtagVersion.Any && asset.Version != version))
        {
            return null;
        }

        return asset;
    }

    private async Task<AssetDomainObject.State?> GetCachedAsync(DomainId uniqueId, long version,
        CancellationToken ct)
    {
        using (Telemetry.Activities.StartActivity("AssetLoader/GetCachedAsync"))
        {
            return await domainObjectCache.GetAsync<AssetDomainObject.State>(uniqueId, version, ct);
        }
    }

    private async Task<AssetDomainObject.State> GetAsync(DomainId uniqueId, long version,
        CancellationToken ct)
    {
        using (Telemetry.Activities.StartActivity("AssetLoader/GetAsync"))
        {
            var contentObject = domainObjectFactory.Create<AssetDomainObject>(uniqueId);

            return await contentObject.GetSnapshotAsync(version, ct);
        }
    }
}
