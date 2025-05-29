// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq.Expressions;
using Google.Protobuf;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PhenX.EntityFrameworkCore.BulkInsert.Extensions;
using PhenX.EntityFrameworkCore.BulkInsert.Options;
using Squidex.Domain.Apps.Entities;
using Squidex.Infrastructure.Queries;
using Squidex.Infrastructure.States;

namespace Squidex.Infrastructure;

public static class Extensions
{
    public static IServiceCollection AddNamedDbContext<TContext>(this IServiceCollection services,
        Action<DbContextOptionsBuilder<TContext>, string> configure)
         where TContext : DbContext
    {
        services.AddSingleton<IDbContextNamedFactory<TContext>>(c =>
            ActivatorUtilities.CreateInstance<PooledDbNamedContextFactory<TContext>>(c, configure));

        return services;
    }

    public static DbContextOptionsBuilder<TContext> UsePrefix<TContext>(this DbContextOptionsBuilder<TContext> builder, string prefix)
        where TContext : DbContext
    {
        ((IDbContextOptionsBuilderInfrastructure)builder).AddOrUpdateExtension(new PrefixExtension(prefix));
        return builder;
    }

    public static DbContextOptionsBuilder<TContext> UsePoolSize<TContext>(this DbContextOptionsBuilder<TContext> builder, int poolSize)
        where TContext : DbContext
    {
        var extension =
            (builder.Options.FindExtension<CoreOptionsExtension>() ?? new CoreOptionsExtension())
                .WithMaxPoolSize(poolSize);

        ((IDbContextOptionsBuilderInfrastructure)builder).AddOrUpdateExtension(extension);
        return builder;
    }

    public static string Prefix(this DbContextOptions options)
    {
        return options.GetExtension<PrefixExtension>().Prefix;
    }

    public static DbContextOptionsBuilder SetDefaultWarnings(this DbContextOptionsBuilder builder)
    {
        builder.ConfigureWarnings(w => w.Ignore(CoreEventId.CollectionWithoutComparer));
        return builder;
    }

    public static IQueryable<T> Pagination<T>(this IQueryable<T> source, ClrQuery query)
    {
        if (query.Skip > 0)
        {
            source = source.Skip((int)query.Skip);
        }

        if (query.Take < long.MaxValue)
        {
            source = source.Take((int)query.Take);
        }

        return source;
    }

    public static IQueryable<T> WhereIf<T>(this IQueryable<T> source, Expression<Func<T, bool>> predicate, bool valid)
    {
        if (!valid)
        {
            return source;
        }

        return source.Where(predicate);
    }

    public static Task BulkUpsertAsync<T>(this DbContext dbContext, List<T> source,
        CancellationToken ct) where T : class
    {
        if (source.Count == 0)
        {
            return Task.CompletedTask;
        }

        return dbContext.ExecuteBulkInsertAsync(source, o => { }, new OnConflictOptions<T> { Update = e => e }, ct);
    }

    public static Task BulkInsertAsync<T>(this DbContext dbContext, List<T> source,
        CancellationToken ct) where T : class
    {
        if (source.Count == 0)
        {
            return Task.CompletedTask;
        }

        return dbContext.ExecuteBulkInsertAsync(source, cancellationToken: ct);
    }

    public static async Task<IResultList<T>> QueryAsync<T>(this IQueryable<T> queryable, Q q,
        CancellationToken ct) where T : class
    {
        var query = q.Query;

        var queryEntities = await queryable.Pagination(q.Query).ToListAsync(ct);
        var queryTotal = (long)queryEntities.Count;

        if (queryEntities.Count >= query.Take || query.Skip > 0)
        {
            if (q.NoTotal)
            {
                queryTotal = -1;
            }
            else
            {
                queryTotal = await queryable.CountAsync(ct);
            }
        }

        if (q.Query.Random > 0)
        {
            queryEntities = queryEntities.TakeRandom(q.Query.Random).ToList();
        }

        return ResultList.Create(queryTotal, queryEntities.OfType<T>());
    }

    public static async Task<IResultList<T>> QueryAsync<T>(this DbContext dbContext, SqlQueryBuilder sqlQuery, Q q,
        CancellationToken ct) where T : class
    {
        sqlQuery.Limit(q.Query);
        sqlQuery.Offset(q.Query);
        sqlQuery.Order(q.Query);
        sqlQuery.Where(q.Query);

        var (sql, parameters) = sqlQuery.Compile();

        var queryEntities = await dbContext.Set<T>().FromSqlRaw(sql, parameters).ToListAsync(ct);
        var queryTotal = (long)queryEntities.Count;

        if (queryEntities.Count >= q.Query.Take || q.Query.Skip > 0)
        {
            if (q.NoTotal || q.NoSlowTotal)
            {
                queryTotal = -1;
            }
            else
            {
                var (countSql, countParams) = sqlQuery.Count().Compile();

                queryTotal =
                    await dbContext.Database.SqlQueryRaw<int>(countSql, countParams)
                        .FirstOrDefaultAsync(ct);
            }
        }

        if (q.Query.Random > 0)
        {
            queryEntities = queryEntities.TakeRandom(q.Query.Random).ToList();
        }

        return ResultList.Create(queryTotal, queryEntities.OfType<T>());
    }

    public static async Task UpsertAsync<T>(this DbContext dbContext, T entity, long oldVersion,
        Func<T, Expression<Func<SetPropertyCalls<T>, SetPropertyCalls<T>>>> update,
        CancellationToken ct) where T : class, IVersionedEntity<DomainId>
    {
        var dbSet = dbContext.Set<T>();
        try
        {
            await dbSet.AddAsync(entity, ct);
            await dbContext.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            var updateQuery = dbSet.Where(x => x.DocumentId == entity.DocumentId);
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
                    await dbSet
                        .Where(x => x.DocumentId == entity.DocumentId).Select(x => x.Version)
                        .ToListAsync(ct);

                var current = currentVersions.Count == 1 ? currentVersions[0] : EtagVersion.Empty;

                throw new InconsistentStateException(current, oldVersion);
            }
        }
        finally
        {
            dbContext.Entry(entity).State = EntityState.Detached;
        }
    }
}
