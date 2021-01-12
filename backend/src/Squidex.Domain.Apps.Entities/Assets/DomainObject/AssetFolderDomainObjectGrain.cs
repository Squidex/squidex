// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Orleans;

namespace Squidex.Domain.Apps.Entities.Assets.DomainObject
{
    public sealed class AssetFolderDomainObjectGrain : DomainObjectGrain<AssetFolderDomainObject, AssetFolderDomainObject.State>, IAssetFolderGrain
    {
        private static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(5);

        public AssetFolderDomainObjectGrain(IServiceProvider serviceProvider, IActivationLimit limit)
            : base(serviceProvider)
        {
            limit?.SetLimit(5000, Lifetime);
        }

        protected override Task OnActivateAsync(string key)
        {
            TryDelayDeactivation(Lifetime);

            return base.OnActivateAsync(key);
        }

        public async Task<J<IAssetFolderEntity>> GetStateAsync()
        {
            await DomainObject.EnsureLoadedAsync();

            return Snapshot;
        }
    }
}
