// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Schemas.DomainObject;
using Squidex.Infrastructure.Migrations;
using Squidex.Infrastructure.States;

namespace Migrations.Migrations
{
    public sealed class ClearSchemas : IMigration
    {
        private readonly IStore<SchemaDomainObject.State> store;

        public ClearSchemas(IStore<SchemaDomainObject.State> store)
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
