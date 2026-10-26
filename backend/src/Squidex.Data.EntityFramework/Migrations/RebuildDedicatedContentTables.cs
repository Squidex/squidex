// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Migrations;

namespace Squidex.Migrations;

public sealed class RebuildDedicatedContentTables<TContext, TContentContext>(
    IDbContextFactory<TContext> dbContextFactory,
    IDbContextNamedFactory<TContentContext> dbContentContextFactory,
    IOptions<ContentsOptions> options,
    ILogger<RebuildDedicatedContentTables<TContext, TContentContext>> log)
    : IMigration
    where TContext : DbContext where TContentContext : ContentDbContext
{
    public const string Key = "RebuildDedicatedContentTables";

    public async Task UpdateAsync(
        CancellationToken ct)
    {
        if (!options.Value.OptimizeForSelfHosting)
        {
            return;
        }

        var dynamicTables = new DynamicTables<TContext, TContentContext>(dbContextFactory, dbContentContextFactory);

        // All dedicated tables shared the model of the first table, therefore the contents are in the wrong tables.
        var names = await dynamicTables.GetContextNames(ct).ToHashSetAsync(cancellationToken: ct);

        await using (var dbContext = await dbContextFactory.CreateDbContextAsync(ct))
        {
            var schemas =
                await dbContext.Set<EFContentCompleteEntity>()
                    .Select(x => new { x.IndexedAppId, x.IndexedSchemaId }).Distinct()
                    .ToListAsync(ct);

            foreach (var schema in schemas)
            {
                names.Add(new DynamicContextName(schema.IndexedAppId, schema.IndexedSchemaId));
            }
        }

        foreach (var name in names)
        {
            await using var contentContext = await dynamicTables.CreateDbContextAsync(name, ct);
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(ct);

            await using var transaction = await contentContext.Database.BeginTransactionAsync(ct);

            await RebuildAsync<EFContentCompleteEntity>(dbContext, contentContext, name, ct);
            await RebuildAsync<EFContentPublishedEntity>(dbContext, contentContext, name, ct);

            await transaction.CommitAsync(ct);

            log.LogInformation("Rebuilt dedicated content tables for app {appId} and schema {schemaId}.", name.AppId, name.SchemaId);
        }
    }

    private static async Task RebuildAsync<T>(DbContext dbContext, DbContext contentContext, DynamicContextName name,
        CancellationToken ct) where T : EFContentEntity
    {
        await contentContext.Set<T>().ExecuteDeleteAsync(ct);

        var sourceType = dbContext.Model.FindEntityType(typeof(T))!;
        var sourceTable = StoreObjectIdentifier.Table(sourceType.GetTableName()!, sourceType.GetSchema());

        var targetType = contentContext.Model.FindEntityType(typeof(T))!;
        var targetTable = StoreObjectIdentifier.Table(targetType.GetTableName()!, targetType.GetSchema());

        var sqlHelper = contentContext.GetService<ISqlGenerationHelper>();

        // The column order can differ between the tables, therefore the columns are listed explicitly.
        var sqlColumns = string.Join(", ", targetType.GetProperties().Select(x => sqlHelper.DelimitIdentifier(x.GetColumnName(targetTable)!)));
        var sqlAppId = sqlHelper.DelimitIdentifier(sourceType.FindProperty(nameof(EFContentEntity.IndexedAppId))!.GetColumnName(sourceTable)!);
        var sqlSchemaId = sqlHelper.DelimitIdentifier(sourceType.FindProperty(nameof(EFContentEntity.IndexedSchemaId))!.GetColumnName(sourceTable)!);

        var sqlInsert = $$"""
            INSERT INTO {{sqlHelper.DelimitIdentifier(targetTable.Name, targetTable.Schema)}} ({{sqlColumns}})
            SELECT {{sqlColumns}}
            FROM {{sqlHelper.DelimitIdentifier(sourceTable.Name, sourceTable.Schema)}}
            WHERE {{sqlAppId}} = {0} AND {{sqlSchemaId}} = {1}
            """;

        await contentContext.Database.ExecuteSqlRawAsync(sqlInsert, [name.AppId.ToString(), name.SchemaId.ToString()], ct);
    }
}
