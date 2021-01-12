// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Rules.DomainObject;
using Squidex.Infrastructure.Migrations;
using Squidex.Infrastructure.States;

namespace Migrations.Migrations
{
    public sealed class ClearRules : IMigration
    {
        private readonly IStore<Guid> store;

        public ClearRules(IStore<Guid> store)
        {
            this.store = store;
        }

        public Task UpdateAsync()
        {
            return store.ClearSnapshotsAsync<Guid, RuleDomainObject.State>();
        }
    }
}
