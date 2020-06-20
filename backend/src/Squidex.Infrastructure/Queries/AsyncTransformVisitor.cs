// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Squidex.Infrastructure.Queries
{
    public abstract class AsyncTransformVisitor<TValue> : FilterNodeVisitor<ValueTask<FilterNode<TValue>?>, TValue>
    {
        public override ValueTask<FilterNode<TValue>?> Visit(CompareFilter<TValue> nodeIn)
        {
            return new ValueTask<FilterNode<TValue>?>(nodeIn);
        }

        public override async ValueTask<FilterNode<TValue>?> Visit(LogicalFilter<TValue> nodeIn)
        {
            var pruned = new List<FilterNode<TValue>>(nodeIn.Filters.Count);

            foreach (var inner in nodeIn.Filters)
            {
                var transformed = await inner.Accept(this);

                if (transformed != null)
                {
                    pruned.Add(transformed);
                }
            }

            return new LogicalFilter<TValue>(nodeIn.Type, pruned);
        }

        public override async ValueTask<FilterNode<TValue>?> Visit(NegateFilter<TValue> nodeIn)
        {
            var inner = await nodeIn.Filter.Accept(this);

            if (inner == null)
            {
                return inner;
            }

            return new NegateFilter<TValue>(inner);
        }
    }
}
