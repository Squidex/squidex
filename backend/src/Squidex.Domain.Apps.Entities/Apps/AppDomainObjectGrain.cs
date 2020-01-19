// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Apps.State;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Orleans;

namespace Squidex.Domain.Apps.Entities.Apps
{
    public sealed class AppDomainObjectGrain : DomainObjectGrain<AppDomainObject, AppState>, IAppGrain
    {
        public AppDomainObjectGrain(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        public async Task<J<IAppEntity>> GetStateAsync()
        {
            await DomainObject.EnsureLoadedAsync();

            return Snapshot;
        }
    }
}
