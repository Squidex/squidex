// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Infrastructure.Queries;
using Squidex.Text;

namespace Squidex.Domain.Apps.Entities.Assets;

internal sealed class AssetSqlQueryBuilder(SqlDialect dialect) : SqlQueryBuilder(dialect, "Assets")
{
    public override string Visit(CompareFilter<ClrValue> nodeIn, None args)
    {
        if (!IsTagsField(nodeIn.Path))
        {
            return base.Visit(nodeIn, args);
        }

        switch (nodeIn.Operator)
        {
            case CompareOperator.Equals when nodeIn.Value.Value is string value:
                return Visit(ClrFilter.Contains(nodeIn.Path, TagsConverter.FormatFilter(value)), args);
            case CompareOperator.NotEquals when nodeIn.Value.Value is string value:
                return Visit(
                    ClrFilter.Not(
                        ClrFilter.Contains(nodeIn.Path, TagsConverter.FormatFilter(value))
                    ),
                    args);
            case CompareOperator.In when nodeIn.Value.Value is List<string> values:
                return Visit(
                    ClrFilter.Or(
                        values.Select(v =>
                            ClrFilter.Contains(nodeIn.Path, TagsConverter.FormatFilter(v))
                        ).ToArray()
                    ),
                    args);
        }

        return base.Visit(nodeIn, args);
    }

    public override PropertyPath Visit(PropertyPath path)
    {
        var elements = path.ToList();

        elements[0] = elements[0].ToPascalCase();

        return new PropertyPath(elements);
    }

    public override bool IsJsonPath(PropertyPath path)
    {
        return path.Count > 1 && string.Equals(path[0], "metadata", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsTagsField(PropertyPath path)
    {
        return path.Count == 1 && string.Equals(path[0], "tags", StringComparison.OrdinalIgnoreCase);
    }
}
