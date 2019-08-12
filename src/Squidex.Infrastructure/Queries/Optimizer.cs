// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;

namespace Squidex.Infrastructure.Queries
{
    public sealed class Optimizer<TValue> : TransformVisitor<TValue>
    {
        private static readonly Optimizer<TValue> Instance = new Optimizer<TValue>();

        private Optimizer()
        {
        }

        public static FilterNode<TValue> Optimize(FilterNode<TValue> source)
        {
            return source?.Accept(Instance);
        }

        public override FilterNode<TValue> Visit(LogicalFilter<TValue> nodeIn)
        {
            var pruned = nodeIn.Filters.Select(x => x.Accept(this)).Where(x => x != null).ToList();

            if (pruned.Count == 1)
            {
                return pruned[0];
            }

            if (pruned.Count == 0)
            {
                return null;
            }

            return new LogicalFilter<TValue>(nodeIn.Type, pruned);
        }

        public override FilterNode<TValue> Visit(NegateFilter<TValue> nodeIn)
        {
            var pruned = nodeIn.Filter.Accept(this);

            if (pruned == null)
            {
                return null;
            }

            if (pruned is CompareFilter<TValue> comparison)
            {
                if (comparison.Operator == CompareOperator.Equals)
                {
                    return new CompareFilter<TValue>(comparison.Path, CompareOperator.NotEquals, comparison.Value);
                }

                if (comparison.Operator == CompareOperator.NotEquals)
                {
                    return new CompareFilter<TValue>(comparison.Path, CompareOperator.Equals, comparison.Value);
                }
            }

            return new NegateFilter<TValue>(pruned);
        }
    }
}
