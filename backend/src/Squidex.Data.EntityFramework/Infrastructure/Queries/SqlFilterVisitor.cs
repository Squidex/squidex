// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.Queries;

public sealed class SqlFilterVisitor(SqlDialect dialect)
    : FilterNodeVisitor<string, ClrValue, (SqlQueryBuilder Builder, SqlParameters Parameters)>
{
    public override string Visit(CompareFilter<ClrValue> nodeIn, (SqlQueryBuilder Builder, SqlParameters Parameters) args)
    {
        return dialect.Where(
            args.Builder.AdaptPath(nodeIn.Path),
            nodeIn.Operator,
            nodeIn.Value,
            args.Parameters,
            args.Builder.IsJsonPath(nodeIn.Path));
    }

    public override string Visit(LogicalFilter<ClrValue> nodeIn, (SqlQueryBuilder Builder, SqlParameters Parameters) args)
    {
        var parts = nodeIn.Filters.Select(x => x.Accept(this, args));

        if (nodeIn.Type == LogicalFilterType.And)
        {
            return dialect.And(parts);
        }

        return dialect.Or(parts);
    }

    public override string Visit(NegateFilter<ClrValue> nodeIn, (SqlQueryBuilder Builder, SqlParameters Parameters) args)
    {
        return dialect.Not(nodeIn.Filter.Accept(this, args));
    }
}
