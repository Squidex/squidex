// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Squidex.Infrastructure.States;

namespace Squidex.Infrastructure;

public static class Extensions
{
    public static IQueryable<T> WhereIf<T>(this IQueryable<T> source, Expression<Func<T, bool>> predicate, bool valid)
    {
        if (!valid)
        {
            return source;
        }

        return source.Where(predicate);
    }

    public static async Task UpsertAsync<T>(this DbContext dbContext, T entity, long oldVersion,
        Func<T, Expression<Func<SetPropertyCalls<T>, SetPropertyCalls<T>>>> update,
        CancellationToken ct) where T : class, IVersionedEntity<DomainId>
    {
        try
        {
            await dbContext.Set<T>().AddAsync(entity, ct);
            await dbContext.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            var updateQuery = dbContext.Set<T>().Where(x => x.DocumentId == entity.DocumentId);
            if (oldVersion > EtagVersion.Any)
            {
                updateQuery = updateQuery.Where(x => x.Version == oldVersion);
            }

            var updateCount =
                await updateQuery
                    .ExecuteUpdateAsync(update(entity), ct);

            if (updateCount != 1)
            {
                var currentVersions =
                    await dbContext.Set<T>()
                        .Where(x => x.DocumentId == entity.DocumentId).Select(x => x.Version)
                        .ToListAsync(ct);

                var current = currentVersions.Count == 1 ? currentVersions[0] : EtagVersion.Empty;

                throw new InconsistentStateException(current, oldVersion);
            }
        }
    }
}
