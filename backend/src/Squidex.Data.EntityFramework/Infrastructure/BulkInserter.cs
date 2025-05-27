// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using PhenX.EntityFrameworkCore.BulkInsert.Extensions;
using PhenX.EntityFrameworkCore.BulkInsert.Options;
using Squidex.Events.EntityFramework;
using Squidex.Flows.EntityFramework;

namespace Squidex.Infrastructure;

public sealed class BulkInserter : IDbFlowsBulkInserter, IDbEventStoreBulkInserter
{
    public Task BulkInsertAsync<T>(DbContext dbContext, IEnumerable<T> entities,
        CancellationToken ct = default) where T : class
    {
        return dbContext.ExecuteBulkInsertAsync(entities, cancellationToken: ct);
    }

    public Task BulkUpsertAsync<T>(DbContext dbContext, IEnumerable<T> entities,
        CancellationToken ct = default) where T : class
    {
        return dbContext.ExecuteBulkInsertAsync(entities, o => { }, new OnConflictOptions<T> { Update = e => e }, ct);
    }
}
