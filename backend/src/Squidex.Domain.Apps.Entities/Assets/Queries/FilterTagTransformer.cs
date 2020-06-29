// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Tags;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Queries;

namespace Squidex.Domain.Apps.Entities.Assets.Queries
{
    public sealed class FilterTagTransformer : AsyncTransformVisitor<ClrValue>
    {
        private readonly ITagService tagService;
        private readonly DomainId appId;

        private FilterTagTransformer(DomainId appId, ITagService tagService)
        {
            this.appId = appId;

            this.tagService = tagService;
        }

        public static ValueTask<FilterNode<ClrValue>?> TransformAsync(FilterNode<ClrValue> nodeIn, DomainId appId, ITagService tagService)
        {
            Guard.NotNull(nodeIn, nameof(nodeIn));
            Guard.NotNull(tagService, nameof(tagService));

            return nodeIn.Accept(new FilterTagTransformer(appId, tagService));
        }

        public override async ValueTask<FilterNode<ClrValue>?> Visit(CompareFilter<ClrValue> nodeIn)
        {
            if (string.Equals(nodeIn.Path[0], nameof(IAssetEntity.Tags), StringComparison.OrdinalIgnoreCase) && nodeIn.Value.Value is string stringValue)
            {
                var tagNames = await tagService.GetTagIdsAsync(appId, TagGroups.Assets, HashSet.Of(stringValue));

                if (tagNames.TryGetValue(stringValue, out var normalized))
                {
                    return new CompareFilter<ClrValue>(nodeIn.Path, nodeIn.Operator, normalized);
                }
            }

            return nodeIn;
        }
    }
}
