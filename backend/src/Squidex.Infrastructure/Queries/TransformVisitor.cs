// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.Queries;

public abstract class TransformVisitor<TValue, TArgs> : FilterNodeVisitor<FilterNode<TValue>?, TValue, TArgs>
{
    public override FilterNode<TValue>? Visit(CompareFilter<TValue> nodeIn, TArgs args)
    {
        return nodeIn;
    }

    public override FilterNode<TValue>? Visit(LogicalFilter<TValue> nodeIn, TArgs args)
    {
        var pruned = new List<FilterNode<TValue>>(nodeIn.Filters.Count);

        foreach (var inner in nodeIn.Filters)
        {
            var transformed = inner.Accept(this, args);

            if (transformed != null)
            {
                pruned.Add(transformed);
            }
        }

        return new LogicalFilter<TValue>(nodeIn.Type, pruned);
    }

    public override FilterNode<TValue>? Visit(NegateFilter<TValue> nodeIn, TArgs args)
    {
        var inner = nodeIn.Filter.Accept(this, args);

        if (inner == null)
        {
            return inner;
        }

        return new NegateFilter<TValue>(inner);
    }
}
