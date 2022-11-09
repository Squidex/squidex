// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.Queries;

public sealed class Optimizer<TValue> : TransformVisitor<TValue, None>
{
    private static readonly Optimizer<TValue> Instance = new Optimizer<TValue>();

    private Optimizer()
    {
    }

    public static FilterNode<TValue>? Optimize(FilterNode<TValue> source)
    {
        return source?.Accept(Instance, None.Value);
    }

    public override FilterNode<TValue>? Visit(LogicalFilter<TValue> nodeIn, None args)
    {
        var pruned = new List<FilterNode<TValue>>(nodeIn.Filters.Count);

        foreach (var filter in nodeIn.Filters)
        {
            var transformed = filter.Accept(this, None.Value);

            if (transformed != null)
            {
                pruned.Add(transformed);
            }
        }

        if (pruned.Count == 1)
        {
            return pruned[0];
        }

        if (pruned.Count == 0)
        {
            return null;
        }

        return nodeIn with { Filters = pruned };
    }

    public override FilterNode<TValue>? Visit(NegateFilter<TValue> nodeIn, None args)
    {
        var pruned = nodeIn.Filter.Accept(this, None.Value);

        if (pruned == null)
        {
            return null;
        }

        if (pruned is CompareFilter<TValue> comparison)
        {
            if (comparison.Operator == CompareOperator.Equals)
            {
                return comparison with { Operator = CompareOperator.NotEquals };
            }

            if (comparison.Operator == CompareOperator.NotEquals)
            {
                return comparison with { Operator = CompareOperator.Equals };
            }
        }

        if (ReferenceEquals(pruned, nodeIn.Filter))
        {
            return nodeIn;
        }

        return new NegateFilter<TValue>(pruned);
    }
}
