// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.Queries;

public class SqlQueryBuilder(SqlDialect dialect)
{
    private readonly SqlFilterVisitor visitor = new SqlFilterVisitor(dialect);

    public (string Sql, object[] Parameters) BuildQuery(string table, ClrQuery query)
    {
        Guard.NotNullOrEmpty(table);
        Guard.NotNull(query);

        var sqlQuery = new SqlQuery
        {
            Table = table,
            Offset = query.Skip,
            Order = [],
            Limit = query.Take,
        };

        if (query.Sort != null)
        {
            foreach (var sort in query.Sort)
            {
                sqlQuery.Order.Add(
                    dialect.OrderBy(
                        AdaptPath(sort.Path),
                        sort.Order,
                        IsJsonPath(sort.Path)));
            }
        }

        var parameters = new SqlParameters();

        if (query.Filter != null)
        {
            sqlQuery.Where.Add(query.Filter.Accept(visitor, (this, parameters)));
        }

        return (dialect.BuildSelectStatement(sqlQuery), parameters.ToArray());
    }

    public virtual bool IsJsonPath(PropertyPath path)
    {
        return false;
    }

    public virtual PropertyPath AdaptPath(PropertyPath path)
    {
        return path;
    }
}
