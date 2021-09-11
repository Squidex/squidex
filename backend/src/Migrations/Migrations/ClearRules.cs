// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Rules.DomainObject;
using Squidex.Infrastructure.Migrations;
using Squidex.Infrastructure.States;

namespace Migrations.Migrations
{
    public sealed class ClearRules : IMigration
    {
        private readonly IStore<RuleDomainObject.State> store;

        public ClearRules(IStore<RuleDomainObject.State> store)
        {
            this.store = store;
        }

        public Task UpdateAsync(
            CancellationToken ct)
        {
            return store.ClearSnapshotsAsync();
        }
    }
}
