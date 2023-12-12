// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules;
using Squidex.Infrastructure.Migrations;
using Squidex.Infrastructure.States;

namespace Migrations.Migrations;

public sealed class ClearRules : IMigration
{
    private readonly IStore<Rule> store;

    public ClearRules(IStore<Rule> store)
    {
        this.store = store;
    }

    public Task UpdateAsync(
        CancellationToken ct)
    {
        return store.ClearSnapshotsAsync();
    }
}
