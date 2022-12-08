// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Tags;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Queries;

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Squidex.Domain.Apps.Entities.Assets.Queries;

internal sealed class FilterTagTransformer : AsyncTransformVisitor<ClrValue, FilterTagTransformer.Args>
{
    private static readonly FilterTagTransformer Instance = new FilterTagTransformer();

    public record struct Args(DomainId AppId, ITagService TagService, CancellationToken CancellationToken);

    private FilterTagTransformer()
    {
    }

    public static ValueTask<FilterNode<ClrValue>?> TransformAsync(FilterNode<ClrValue> nodeIn, DomainId appId, ITagService tagService,
        CancellationToken ct)
    {
        var args = new Args(appId, tagService, ct);

        return nodeIn.Accept(Instance, args);
    }

    public override async ValueTask<FilterNode<ClrValue>?> Visit(CompareFilter<ClrValue> nodeIn, Args args)
    {
        if (string.Equals(nodeIn.Path[0], nameof(IAssetEntity.Tags), StringComparison.OrdinalIgnoreCase) && nodeIn.Value.Value is string stringValue)
        {
            var tagNames = await args.TagService.GetTagIdsAsync(args.AppId, TagGroups.Assets, HashSet.Of(stringValue), args.CancellationToken);

            if (tagNames.TryGetValue(stringValue, out var normalized))
            {
                return new CompareFilter<ClrValue>(nodeIn.Path, nodeIn.Operator, normalized);
            }
        }

        return nodeIn;
    }
}
