// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;

namespace Squidex.Infrastructure.Queries;

public class SqlQueryBuilder(SqlDialect dialect, string table) : FilterNodeVisitor<string, ClrValue, None>
{
    private readonly SqlQuery sqlQuery = new SqlQuery(table);
    private readonly SqlParams parameters = [];

    public SqlQueryBuilder Where(PropertyPath path, CompareOperator op, ClrValue value)
    {
        sqlQuery.Where.Add(dialect.Where(Visit(path), op, value, parameters, IsJsonPath(path)));
        return this;
    }

    public SqlQueryBuilder WithCount()
    {
        sqlQuery.Fields = [dialect.CountAll()];
        return this;
    }

    public SqlQueryBuilder WithLimit(long limit)
    {
        sqlQuery.Limit = limit;
        return this;
    }

    public SqlQueryBuilder WithOffset(long offset)
    {
        sqlQuery.Offset = offset;
        return this;
    }

    public SqlQueryBuilder WithoutOrder()
    {
        sqlQuery.Order = [];
        return this;
    }

    public SqlQueryBuilder WithLimit(ClrQuery query)
    {
        Guard.NotNull(query);
        return WithLimit(query.Take);
    }

    public SqlQueryBuilder WithOffset(ClrQuery query)
    {
        Guard.NotNull(query);
        return WithOffset(query.Skip);
    }

    public SqlQueryBuilder WithFilter(ClrQuery query)
    {
        Guard.NotNull(query);

        if (query.Filter != null)
        {
            sqlQuery.Where.Add(query.Filter.Accept(this, None.Value));
        }

        return this;
    }

    public SqlQueryBuilder WithOrders(ClrQuery query)
    {
        Guard.NotNull(query);

        if (query.Sort != null)
        {
            foreach (var sort in query.Sort)
            {
                sqlQuery.Order.Add(
                    dialect.OrderBy(
                        Visit(sort.Path),
                        sort.Order,
                        IsJsonPath(sort.Path)));
            }
        }

        return this;
    }

    public (string Sql, object[] Parameters) Compile()
    {
        return (dialect.BuildSelectStatement(sqlQuery), parameters.ToArray());
    }

    public virtual bool IsJsonPath(PropertyPath path)
    {
        return false;
    }

    public virtual PropertyPath Visit(PropertyPath path)
    {
        return path;
    }

    public override string Visit(CompareFilter<ClrValue> nodeIn, None args)
    {
        return dialect.Where(
            Visit(nodeIn.Path),
            nodeIn.Operator,
            nodeIn.Value,
            parameters,
            IsJsonPath(nodeIn.Path));
    }

    public override string Visit(LogicalFilter<ClrValue> nodeIn, None args)
    {
        var parts = nodeIn.Filters.Select(x => x.Accept(this, args));

        if (nodeIn.Type == LogicalFilterType.And)
        {
            return dialect.And(parts);
        }

        return dialect.Or(parts);
    }

    public override string Visit(NegateFilter<ClrValue> nodeIn, None args)
    {
        return dialect.Not(nodeIn.Filter.Accept(this, args));
    }
}
