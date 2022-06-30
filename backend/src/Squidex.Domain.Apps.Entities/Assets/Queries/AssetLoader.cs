// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.DependencyInjection;
using Squidex.Domain.Apps.Entities.Assets.DomainObject;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Assets.Queries
{
    public sealed class AssetLoader : IAssetLoader
    {
        private readonly Func<DomainId, AssetDomainObject> factory;

        public AssetLoader(IServiceProvider serviceProvider)
        {
            var objectFactory = ActivatorUtilities.CreateFactory(typeof(AssetDomainObject), new[] { typeof(DomainId) });

            factory = key =>
            {
                return (AssetDomainObject)objectFactory(serviceProvider, new object[] { key });
            };
        }

        public async Task<IAssetEntity?> GetAsync(DomainId appId, DomainId id, long version = EtagVersion.Any)
        {
            using (Telemetry.Activities.StartActivity("AssetLoader/GetAsync"))
            {
                var key = DomainId.Combine(appId, id);

                var assetObject = factory(key);
                var assetState = await assetObject.GetSnapshotAsync(version);

                if (assetState == null || assetState.Version <= EtagVersion.Empty || (version > EtagVersion.Any && assetState.Version != version))
                {
                    return null;
                }

                return assetState;
            }
        }
    }
}
