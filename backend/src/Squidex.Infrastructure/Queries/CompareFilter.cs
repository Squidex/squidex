// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;

namespace Squidex.Infrastructure.Queries
{
    public sealed class CompareFilter<TValue> : FilterNode<TValue>
    {
        public PropertyPath Path { get; }

        public CompareOperator Operator { get; }

        public TValue Value { get; }

        public CompareFilter(PropertyPath path, CompareOperator @operator, TValue value)
        {
            Guard.NotNull(path, nameof(path));
            Guard.NotNull(value, nameof(value));
            Guard.Enum(@operator, nameof(@operator));

            Path = path;

            Operator = @operator;

            Value = value;
        }

        public override void AddFields(HashSet<string> fields)
        {
            fields.Add(Path.ToString());
        }

        public override T Accept<T>(FilterNodeVisitor<T, TValue> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            switch (Operator)
            {
                case CompareOperator.Contains:
                    return $"contains({Path}, {Value})";
                case CompareOperator.Empty:
                    return $"empty({Path})";
                case CompareOperator.EndsWith:
                    return $"endsWith({Path}, {Value})";
                case CompareOperator.StartsWith:
                    return $"startsWith({Path}, {Value})";
                case CompareOperator.Equals:
                    return $"{Path} == {Value}";
                case CompareOperator.NotEquals:
                    return $"{Path} != {Value}";
                case CompareOperator.GreaterThan:
                    return $"{Path} > {Value}";
                case CompareOperator.GreaterThanOrEqual:
                    return $"{Path} >= {Value}";
                case CompareOperator.LessThan:
                    return $"{Path} < {Value}";
                case CompareOperator.LessThanOrEqual:
                    return $"{Path} <= {Value}";
                case CompareOperator.In:
                    return $"{Path} in {Value}";
                default:
                    return string.Empty;
            }
        }
    }
}