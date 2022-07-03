// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Orleans.Core;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Orleans;

namespace Squidex.Domain.Apps.Entities.Assets.DomainObject
{
    public sealed class AssetFolderDomainObjectGrain : DomainObjectGrain<AssetFolderDomainObject, AssetFolderDomainObject.State>, IAssetFolderGrain
    {
        private static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(5);

        public AssetFolderDomainObjectGrain(IGrainIdentity grainIdentity, IDomainObjectFactory factory,
            IActivationLimit limit)
            : base(grainIdentity, factory)
        {
            limit?.SetLimit(5000, Lifetime);
        }

        public override Task OnActivateAsync()
        {
            TryDelayDeactivation(Lifetime);

            return base.OnActivateAsync();
        }

        public async Task<IAssetFolderEntity> GetStateAsync()
        {
            await DomainObject.EnsureLoadedAsync();

            return Snapshot;
        }
    }
}
