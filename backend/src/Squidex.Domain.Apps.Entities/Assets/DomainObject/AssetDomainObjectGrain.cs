// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Orleans.Core;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Orleans;

namespace Squidex.Domain.Apps.Entities.Assets.DomainObject
{
    public sealed class AssetDomainObjectGrain : DomainObjectGrain<AssetDomainObject, AssetDomainObject.State>, IAssetGrain
    {
        private static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(5);

        public AssetDomainObjectGrain(IGrainIdentity identity, IDomainObjectFactory factory,
            IActivationLimit limit)
            : base(identity, factory)
        {
            limit?.SetLimit(5000, Lifetime);
        }

        public override Task OnActivateAsync()
        {
            TryDelayDeactivation(Lifetime);

            return base.OnActivateAsync();
        }

        public async Task<IAssetEntity> GetStateAsync(long version = EtagVersion.Any)
        {
            await DomainObject.EnsureLoadedAsync();

            return await DomainObject.GetSnapshotAsync(version);
        }
    }
}
