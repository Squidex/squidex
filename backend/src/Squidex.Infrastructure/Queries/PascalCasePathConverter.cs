// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Text;

namespace Squidex.Infrastructure.Queries;

public sealed class PascalCasePathConverter<TValue> : TransformVisitor<TValue, None>
{
    private static readonly PascalCasePathConverter<TValue> Instance = new PascalCasePathConverter<TValue>();

    private PascalCasePathConverter()
    {
    }

    public static FilterNode<TValue>? Transform(FilterNode<TValue> node)
    {
        return node.Accept(Instance, None.Value);
    }

    public override FilterNode<TValue>? Visit(CompareFilter<TValue> nodeIn, None args)
    {
        return new CompareFilter<TValue>(nodeIn.Path.Select(x => x.ToPascalCase()).ToList(), nodeIn.Operator, nodeIn.Value);
    }
}
