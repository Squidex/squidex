// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.DependencyInjection;
using Squidex.Domain.Apps.Entities.Assets.DomainObject;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities.Assets.Queries
{
    public sealed class AssetLoader : IAssetLoader
    {
        private readonly IDomainObjectFactory domainObjectFactory;

        public AssetLoader(IDomainObjectFactory domainObjectFactory)
        {
            this.domainObjectFactory = domainObjectFactory;
        }

        public async Task<IAssetEntity?> GetAsync(DomainId appId, DomainId id, long version = EtagVersion.Any,
            CancellationToken ct = default)
        {
            using (Telemetry.Activities.StartActivity("AssetLoader/GetAsync"))
            {
                var key = DomainId.Combine(appId, id);

                var assetObject = domainObjectFactory.Create<AssetDomainObject>(key);
                var assetState = await assetObject.GetSnapshotAsync(version, ct);

                if (assetState == null || assetState.Version <= EtagVersion.Empty || (version > EtagVersion.Any && assetState.Version != version))
                {
                    return null;
                }

                return assetState;
            }
        }
    }
}
