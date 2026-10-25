// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Squidex.Infrastructure;
using Squidex.Infrastructure.States;

#pragma warning disable MA0048 // File name must match type name

namespace Squidex.Domain.Apps.Entities.Contents;

public sealed partial class EFContentRepository<TContext, TContentContext>
{
    public async Task CreateIndexAsync(DomainId appId, DomainId schemaId, IndexDefinition index,
        CancellationToken ct = default)
    {
        // Custom indexes are only supported for dedicated tables, like the dedicated collections in MongoDB.
        if (!dedicatedTables)
        {
            return;
        }

        await using var dbContext = await dynamicTables.CreateDbContextAsync(appId, schemaId, ct);

        await dbContext.CreateCustomIndexAsync<EFContentCompleteEntity>(index, ct);
        await dbContext.CreateCustomIndexAsync<EFContentPublishedEntity>(index, ct);
    }

    public async Task<List<IndexDefinition>> GetIndexesAsync(DomainId appId, DomainId schemaId,
        CancellationToken ct = default)
    {
        if (!dedicatedTables)
        {
            return [];
        }

        await using var dbContext = await dynamicTables.CreateDbContextAsync(appId, schemaId, ct);

        return await dbContext.GetCustomIndexesAsync<EFContentCompleteEntity>(ct);
    }

    public async Task DropIndexAsync(DomainId appId, DomainId schemaId, string name,
        CancellationToken ct = default)
    {
        if (!dedicatedTables)
        {
            return;
        }

        await using var dbContext = await dynamicTables.CreateDbContextAsync(appId, schemaId, ct);

        await dbContext.DropCustomIndexAsync<EFContentCompleteEntity>(name, ct);
        await dbContext.DropCustomIndexAsync<EFContentPublishedEntity>(name, ct);
    }
}
