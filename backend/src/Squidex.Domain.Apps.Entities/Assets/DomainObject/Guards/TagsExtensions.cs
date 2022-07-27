// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Tags;

namespace Squidex.Domain.Apps.Entities.Assets.DomainObject.Guards
{
    public static class TagsExtensions
    {
        public static async Task<HashSet<string>> NormalizeTagsAsync(this AssetOperation operation, HashSet<string> names)
        {
            var tagService = operation.Resolve<ITagService>();

            var normalized = await tagService.GetTagIdsAsync(operation.App.Id, TagGroups.Assets, names);

            return new HashSet<string>(normalized.Values);
        }
    }
}
