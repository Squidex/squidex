// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Infrastructure.Queries;

namespace Squidex.Domain.Apps.Entities.MongoDb.Assets.Visitors;

public sealed class FirstPascalPathConverter<TValue> : TransformVisitor<TValue, None>
{
    private static readonly FirstPascalPathConverter<TValue> Instance = new FirstPascalPathConverter<TValue>();

    private FirstPascalPathConverter()
    {
    }

    public static FilterNode<TValue>? Transform(FilterNode<TValue> node)
    {
        return node.Accept(Instance, None.Value);
    }

    public override FilterNode<TValue>? Visit(CompareFilter<TValue> nodeIn, None args)
    {
        return nodeIn with { Path = nodeIn.Path.ToFirstPascalCase() };
    }
}
