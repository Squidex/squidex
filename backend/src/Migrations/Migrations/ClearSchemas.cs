// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure.Migrations;
using Squidex.Infrastructure.States;

namespace Migrations.Migrations;

public sealed class ClearSchemas : IMigration
{
    private readonly IStore<Schema> store;

    public ClearSchemas(IStore<Schema> store)
    {
        this.store = store;
    }

    public Task UpdateAsync(
        CancellationToken ct)
    {
        return store.ClearSnapshotsAsync();
    }
}
