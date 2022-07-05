// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Orleans.Core;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities.Rules.DomainObject
{
    public sealed class RuleDomainObjectGrain : DomainObjectGrain<RuleDomainObject, RuleDomainObject.State>, IRuleGrain
    {
        public RuleDomainObjectGrain(IGrainIdentity identity, IDomainObjectFactory factory)
            : base(identity, factory)
        {
        }

        public async Task<IRuleEntity> GetStateAsync()
        {
            await DomainObject.EnsureLoadedAsync();

            return Snapshot;
        }
    }
}
