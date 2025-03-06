// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.Queries;

public class SqlQueryBuilder(SqlDialect dialect, string table, SqlParams? parameters = null) : FilterNodeVisitor<string, ClrValue, None>
{
    private readonly SqlQuery sqlQuery = new SqlQuery(table);
    private readonly SqlParams sqlParameters = parameters ?? [];

    public SqlQueryBuilder Count()
    {
        sqlQuery.Fields = [dialect.CountAll()];
        sqlQuery.Order.Clear();
        sqlQuery.Offset = 0;
        sqlQuery.Limit = long.MaxValue;
        return this;
    }

    public SqlQueryBuilder WhereQuery(PropertyPath path, CompareOperator op, Func<SqlParams, SqlQueryBuilder> factory)
    {
        var builder = factory(sqlParameters);

        sqlQuery.Where.Add(dialect.WhereQuery(Visit(path), op, builder.CompileQuery(), IsJsonPath(path)));
        return this;
    }

    public SqlQueryBuilder Where(FilterNode<ClrValue> filter)
    {
        sqlQuery.Where.Add(filter.Accept(this, None.Value));
        return this;
    }

    public SqlQueryBuilder WhereMatch(PropertyPath path, string query)
    {
        sqlQuery.Where.Add(dialect.WhereMatch(Visit(path), query, sqlParameters));
        return this;
    }

    public SqlQueryBuilder Order(PropertyPath path, SortOrder order)
    {
        sqlQuery.Order.Add(dialect.OrderBy(Visit(path), order, IsJsonPath(path)));
        return this;
    }

    public SqlQueryBuilder Select(PropertyPath path)
    {
        sqlQuery.Fields.Add(dialect.Field(Visit(path), IsJsonPath(path)));
        return this;
    }

    public SqlQueryBuilder Limit(long limit)
    {
        sqlQuery.Limit = limit;
        return this;
    }

    public SqlQueryBuilder Offset(long offset)
    {
        sqlQuery.Offset = offset;
        return this;
    }

    public SqlQueryBuilder OrderAsc(PropertyPath path)
    {
        return Order(path, SortOrder.Ascending);
    }

    public SqlQueryBuilder OrderDesc(PropertyPath path)
    {
        return Order(path, SortOrder.Descending);
    }

    public SqlQueryBuilder Limit(ClrQuery query)
    {
        return Limit(query.Take);
    }

    public SqlQueryBuilder Offset(ClrQuery query)
    {
        return Offset(query.Skip);
    }

    public SqlQueryBuilder Where(ClrQuery query)
    {
        Guard.NotNull(query);

        if (query.Filter != null)
        {
            return Where(query.Filter);
        }

        return this;
    }

    public SqlQueryBuilder Order(ClrQuery query)
    {
        Guard.NotNull(query);

        if (query.Sort != null)
        {
            foreach (var sort in query.Sort)
            {
                Order(sort.Path, sort.Order);
            }
        }

        return this;
    }

    public (string Sql, object[] Parameters) Compile()
    {
        return (CompileQuery(), sqlParameters.ToArray());
    }

    public virtual string CompileQuery()
    {
        if (sqlQuery.Fields.Count == 0)
        {
            sqlQuery.Fields.Add(dialect.SelectAll());
        }

        return dialect.BuildSelectStatement(sqlQuery);
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
            sqlParameters,
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
