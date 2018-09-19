// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;

namespace Squidex.Infrastructure.Queries
{
    public sealed class FilterComparison : FilterNode
    {
        public IReadOnlyList<string> Lhs { get; }

        public FilterOperator Operator { get; }

        public FilterValue Rhs { get; }

        public FilterComparison(IReadOnlyList<string> lhs, FilterOperator @operator, FilterValue rhs)
        {
            Guard.NotNull(lhs, nameof(lhs));
            Guard.NotEmpty(lhs, nameof(lhs));
            Guard.Enum(@operator, nameof(@operator));

            Lhs = lhs;
            Rhs = rhs;

            Operator = @operator;
        }

        public override T Accept<T>(FilterNodeVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            var path = string.Join(".", Lhs);

            switch (Operator)
            {
                case FilterOperator.Contains:
                    return $"contains({path}, {Rhs})";
                case FilterOperator.EndsWith:
                    return $"endsWith({path}, {Rhs})";
                case FilterOperator.StartsWith:
                    return $"startsWith({path}, {Rhs})";
                case FilterOperator.Equals:
                    return $"{path} == {Rhs}";
                case FilterOperator.NotEquals:
                    return $"{path} != {Rhs}";
                case FilterOperator.GreaterThan:
                    return $"{path} > {Rhs}";
                case FilterOperator.GreaterThanOrEqual:
                    return $"{path} >= {Rhs}";
                case FilterOperator.LessThan:
                    return $"{path} < {Rhs}";
                case FilterOperator.LessThanOrEqual:
                    return $"{path} <= {Rhs}";
                case FilterOperator.In:
                    return $"{path} in {Rhs}";
                default:
                    return string.Empty;
            }
        }
    }
}