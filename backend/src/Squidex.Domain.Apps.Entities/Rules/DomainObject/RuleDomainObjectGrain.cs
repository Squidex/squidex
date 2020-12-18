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

namespace Squidex.Domain.Apps.Entities.Rules.DomainObject
{
    public sealed class RuleDomainObjectGrain : DomainObjectGrain<RuleDomainObject, RuleDomainObject.State>, IRuleGrain
    {
        public RuleDomainObjectGrain(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        public async Task<J<IRuleEntity>> GetStateAsync()
        {
            await DomainObject.EnsureLoadedAsync();

            return Snapshot;
        }
    }
}
