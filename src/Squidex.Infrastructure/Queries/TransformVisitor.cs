// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;

namespace Squidex.Infrastructure.Queries
{
    public abstract class TransformVisitor<TValue> : FilterNodeVisitor<FilterNode<TValue>, TValue>
    {
        public override FilterNode<TValue> Visit(CompareFilter<TValue> nodeIn)
        {
            return nodeIn;
        }

        public override FilterNode<TValue> Visit(LogicalFilter<TValue> nodeIn)
        {
            return new LogicalFilter<TValue>(nodeIn.Type, nodeIn.Filters.Select(x => x.Accept(this)).ToList());
        }

        public override FilterNode<TValue> Visit(NegateFilter<TValue> nodeIn)
        {
            return new NegateFilter<TValue>(nodeIn.Filter.Accept(this));
        }
    }
}
