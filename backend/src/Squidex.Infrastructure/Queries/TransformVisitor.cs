// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;

namespace Squidex.Infrastructure.Queries
{
    public abstract class TransformVisitor<TValue> : FilterNodeVisitor<FilterNode<TValue>?, TValue>
    {
        public override FilterNode<TValue>? Visit(CompareFilter<TValue> nodeIn)
        {
            return nodeIn;
        }

        public override FilterNode<TValue>? Visit(LogicalFilter<TValue> nodeIn)
        {
            var pruned = new List<FilterNode<TValue>>(nodeIn.Filters.Count);

            foreach (var inner in nodeIn.Filters)
            {
                var transformed = inner.Accept(this);

                if (transformed != null)
                {
                    pruned.Add(transformed);
                }
            }

            return new LogicalFilter<TValue>(nodeIn.Type, pruned);
        }

        public override FilterNode<TValue>? Visit(NegateFilter<TValue> nodeIn)
        {
            var inner = nodeIn.Filter.Accept(this);

            if (inner == null)
            {
                return inner;
            }

            return new NegateFilter<TValue>(inner);
        }
    }
}
