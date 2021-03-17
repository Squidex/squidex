// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Squidex.Infrastructure.Queries
{
    public sealed record CompareFilter<TValue>(PropertyPath Path, CompareOperator Operator, TValue Value) : FilterNode<TValue>
    {
        public override void AddFields(HashSet<string> fields)
        {
            fields.Add(Path.ToString());
        }

        public override T Accept<T, TArgs>(FilterNodeVisitor<T, TValue, TArgs> visitor, TArgs args)
        {
            return visitor.Visit(this, args);
        }

        public override string ToString()
        {
            switch (Operator)
            {
                case CompareOperator.Contains:
                    return $"contains({Path}, {Value})";
                case CompareOperator.Empty:
                    return $"empty({Path})";
                case CompareOperator.Exists:
                    return $"exists({Path})";
                case CompareOperator.Matchs:
                    return $"matchs({Path}, {Value})";
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