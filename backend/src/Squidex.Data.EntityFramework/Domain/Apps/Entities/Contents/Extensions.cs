// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Queries;

namespace Squidex.Domain.Apps.Entities.Contents;

internal static class Extensions
{
    public static ContentSqlQueryBuilder ContentQuery<T>(this DbContext dbContext, SqlParams? parameters = null)
    {
        if (dbContext is not IDbContextWithDialect withDialect)
        {
            throw new InvalidOperationException("Invalid context.");
        }

        var tableName = dbContext.Model.FindEntityType(typeof(T))?.GetTableName()
            ?? throw new InvalidOperationException("Unknown model.");

        return new ContentSqlQueryBuilder(withDialect.Dialect, tableName, parameters);
    }

    public static async Task UpsertAsync<T>(this DbContext dbContext, T entity, long oldVersion,
        CancellationToken ct) where T : EFContentEntity
    {
        await dbContext.UpsertAsync(entity, oldVersion, BuildUpdate, ct);
    }

    public static async Task RemoveAsync<T>(this DbContext dbContext, DomainId id,
        CancellationToken ct) where T : EFContentEntity
    {
        await dbContext.Set<T>().Where(x => x.DocumentId == id).ExecuteDeleteAsync(ct);
    }

    public static async Task UpsertOrDeleteAsync<T>(this DbContext dbContext, T? entity, DomainId id, long oldVersion,
        CancellationToken ct) where T : EFContentEntity
    {
        if (entity?.Status == Status.Published && !entity.IsDeleted)
        {
            await UpsertAsync(dbContext, entity, oldVersion, ct);
        }
        else
        {
            await RemoveAsync<T>(dbContext, id, ct);
        }
    }

    public static Expression<Func<SetPropertyCalls<T>, SetPropertyCalls<T>>> BuildUpdate<T>(T entity) where T : EFContentEntity
    {
        return b => b
            .SetProperty(x => x.AppId, entity.AppId)
            .SetProperty(x => x.Created, entity.Created)
            .SetProperty(x => x.CreatedBy, entity.CreatedBy)
            .SetProperty(x => x.Data, entity.Data)
            .SetProperty(x => x.IndexedAppId, entity.IndexedAppId)
            .SetProperty(x => x.IndexedSchemaId, entity.IndexedSchemaId)
            .SetProperty(x => x.IsDeleted, entity.IsDeleted)
            .SetProperty(x => x.LastModified, entity.LastModified)
            .SetProperty(x => x.LastModifiedBy, entity.LastModifiedBy)
            .SetProperty(x => x.NewData, entity.NewData)
            .SetProperty(x => x.NewStatus, entity.NewStatus)
            .SetProperty(x => x.ScheduledAt, entity.ScheduledAt)
            .SetProperty(x => x.ScheduleJob, entity.ScheduleJob)
            .SetProperty(x => x.SchemaId, entity.SchemaId)
            .SetProperty(x => x.Status, entity.Status)
            .SetProperty(x => x.TranslationStatus, entity.TranslationStatus)
            .SetProperty(x => x.Version, entity.Version);
    }

    public static bool ShouldWritePublished(this WriteContent content)
    {
        return content.CurrentVersion.Status == Status.Published && !content.IsDeleted;
    }

    public static SqlQueryBuilder WhereNotDeleted(this SqlQueryBuilder builder, Query<ClrValue>? query)
    {
        return builder.WhereNotDeleted(query?.Filter);
    }

    public static SqlQueryBuilder WhereNotDeleted(this SqlQueryBuilder builder, FilterNode<ClrValue>? filter)
    {
        if (filter?.HasField("IsDeleted") != true)
        {
            builder.Where(ClrFilter.Eq(nameof(EFContentEntity.IsDeleted), false));
        }

        return builder;
    }

    public static void LimitFields(this ContentData data, IReadOnlySet<string> fields)
    {
        List<string>? toDelete = null;
        foreach (var (key, _) in data)
        {
            if (!fields.Any(x => IsMatch(key, x)))
            {
                toDelete ??= [];
                toDelete.Add(key);
            }
        }

        if (toDelete != null)
        {
            foreach (var key in toDelete)
            {
                data.Remove(key);
            }
        }

        static bool IsMatch(string actual, string filter)
        {
            const string Prefix = "data.";

            var span = filter.AsSpan();
            if (span.Equals(actual, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (span.Length <= Prefix.Length || !span.StartsWith(Prefix, StringComparison.Ordinal))
            {
                return false;
            }

            span = span[Prefix.Length..];

            return span.Equals(actual, StringComparison.Ordinal);
        }
    }
}
