// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;

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
            var inner = nodeIn.Filters.Select(x => x.Accept(this)!).Where(x => x != null).ToList();

            return new LogicalFilter<TValue>(nodeIn.Type, inner);
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
