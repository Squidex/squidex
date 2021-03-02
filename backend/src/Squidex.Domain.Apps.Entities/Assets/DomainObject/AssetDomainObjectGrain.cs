// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Orleans;

namespace Squidex.Domain.Apps.Entities.Assets.DomainObject
{
    public sealed class AssetDomainObjectGrain : DomainObjectGrain<AssetDomainObject, AssetDomainObject.State>, IAssetGrain
    {
        private static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(5);

        public AssetDomainObjectGrain(IServiceProvider serviceProvider, IActivationLimit limit)
            : base(serviceProvider)
        {
            limit?.SetLimit(5000, Lifetime);
        }

        protected override Task OnActivateAsync(string key)
        {
            TryDelayDeactivation(Lifetime);

            return base.OnActivateAsync(key);
        }

        public async Task<J<IAssetEntity>> GetStateAsync(long version = EtagVersion.Any)
        {
            await DomainObject.EnsureLoadedAsync();

            return await DomainObject.GetSnapshotAsync(version);
        }
    }
}
