// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Schemas.State;
using Squidex.Infrastructure.Migrations;
using Squidex.Infrastructure.States;

namespace Migrations.Migrations
{
    public sealed class ClearSchemas : IMigration
    {
        private readonly IStore<Guid> store;

        public ClearSchemas(IStore<Guid> store)
        {
            this.store = store;
        }

        public Task UpdateAsync()
        {
            return store.ClearSnapshotsAsync<Guid, SchemaState>();
        }
    }
}
