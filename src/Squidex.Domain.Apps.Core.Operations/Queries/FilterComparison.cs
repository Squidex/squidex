// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Queries
{
    public sealed class FilterComparison : FilterNode
    {
        public IReadOnlyList<string> Path { get; }

        public FilterOperator Operator { get; }

        public FilterValueType ValueType { get; }

        public object Value { get; }

        public FilterComparison(IReadOnlyList<string> path, FilterOperator @operator, object value, FilterValueType valueType)
        {
            Guard.NotNull(path, nameof(path));
            Guard.NotEmpty(path, nameof(path));
            Guard.Enum(@operator, nameof(@operator));
            Guard.Enum(valueType, nameof(valueType));

            Path = path;

            Value = value;
            ValueType = valueType;

            Operator = @operator;
        }

        public override T Accept<T>(FilterNodeVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            var path = string.Join(".", Path);

            switch (Operator)
            {
                case FilterOperator.Contains:
                    return $"contains({path}, {Value})";
                case FilterOperator.EndsWith:
                    return $"endsWith({path}, {Value})";
                case FilterOperator.StartsWith:
                    return $"startsWith({path}, {Value})";
                case FilterOperator.Equals:
                    return $"{path} == {Value}";
                case FilterOperator.NotEquals:
                    return $"{path} != {Value}";
                case FilterOperator.GreaterThan:
                    return $"{path} > {Value}";
                case FilterOperator.GreaterThanOrEqual:
                    return $"{path} >= {Value}";
                case FilterOperator.LessThan:
                    return $"{path} < {Value}";
                case FilterOperator.LessThanOrEqual:
                    return $"{path} <= {Value}";
                default:
                    return string.Empty;
            }
        }
    }
}