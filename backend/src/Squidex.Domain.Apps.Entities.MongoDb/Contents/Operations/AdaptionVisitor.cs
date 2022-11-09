// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Infrastructure.Queries;

namespace Squidex.Domain.Apps.Entities.MongoDb.Contents.Operations;

internal sealed class AdaptionVisitor : TransformVisitor<ClrValue, None>
{
    private static readonly AdaptionVisitor Instance = new AdaptionVisitor();

    private AdaptionVisitor()
    {
    }

    public static FilterNode<ClrValue>? AdaptFilter(FilterNode<ClrValue> filter)
    {
        return filter.Accept(Instance, None.Value);
    }

    public override FilterNode<ClrValue> Visit(CompareFilter<ClrValue> nodeIn, None args)
    {
        if (string.Equals(nodeIn.Path[0], "id", StringComparison.OrdinalIgnoreCase))
        {
            return nodeIn;
        }

        var path = Adapt.MapPath(nodeIn.Path);

        return nodeIn with { Path = path };
    }
}
