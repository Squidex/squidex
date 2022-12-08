// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.Queries;

public abstract class AsyncTransformVisitor<TValue, TArgs> : FilterNodeVisitor<ValueTask<FilterNode<TValue>?>, TValue, TArgs>
{
    public override ValueTask<FilterNode<TValue>?> Visit(CompareFilter<TValue> nodeIn, TArgs args)
    {
        return new ValueTask<FilterNode<TValue>?>(nodeIn);
    }

    public override async ValueTask<FilterNode<TValue>?> Visit(LogicalFilter<TValue> nodeIn, TArgs args)
    {
        var pruned = new List<FilterNode<TValue>>(nodeIn.Filters.Count);

        foreach (var inner in nodeIn.Filters)
        {
            var transformed = await inner.Accept(this, args);

            if (transformed != null)
            {
                pruned.Add(transformed);
            }
        }

        return new LogicalFilter<TValue>(nodeIn.Type, pruned);
    }

    public override async ValueTask<FilterNode<TValue>?> Visit(NegateFilter<TValue> nodeIn, TArgs args)
    {
        var inner = await nodeIn.Filter.Accept(this, args);

        if (inner == null)
        {
            return inner;
        }

        return new NegateFilter<TValue>(inner);
    }
}
