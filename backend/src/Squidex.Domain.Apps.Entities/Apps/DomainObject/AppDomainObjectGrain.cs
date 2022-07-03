// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Orleans.Core;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities.Apps.DomainObject
{
    public sealed class AppDomainObjectGrain : DomainObjectGrain<AppDomainObject, AppDomainObject.State>, IAppGrain
    {
        public AppDomainObjectGrain(IGrainIdentity identity, IDomainObjectFactory factory)
            : base(identity, factory)
        {
        }

        public async Task<IAppEntity> GetStateAsync()
        {
            await DomainObject.EnsureLoadedAsync();

            return Snapshot;
        }
    }
}
