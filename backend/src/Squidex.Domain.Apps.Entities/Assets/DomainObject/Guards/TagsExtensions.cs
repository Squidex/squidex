// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Tags;

namespace Squidex.Domain.Apps.Entities.Assets.DomainObject.Guards
{
    public static class TagService
    {
        public static async Task<HashSet<string>> NormalizeTags(this AssetOperation operation, HashSet<string> tags)
        {
            var tagService = operation.Resolve<ITagService>();

            var normalized = await tagService.NormalizeTagsAsync(operation.App.Id, TagGroups.Assets, tags, operation.Snapshot.Tags);

            return new HashSet<string>(normalized.Values);
        }

        public static async Task UnsetTags(this AssetOperation operation)
        {
            var tagService = operation.Resolve<ITagService>();

            await tagService.NormalizeTagsAsync(operation.App.Id, TagGroups.Assets, null, operation.Snapshot.Tags);
        }
    }
}
