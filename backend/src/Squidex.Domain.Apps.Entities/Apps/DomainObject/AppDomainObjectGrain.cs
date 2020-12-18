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

namespace Squidex.Domain.Apps.Entities.Apps.DomainObject
{
    public sealed class AppDomainObjectGrain : DomainObjectGrain<AppDomainObject, AppDomainObject.State>, IAppGrain
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
