// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Tags;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Assets.DomainObject.Guards;

public static class TagsExtensions
{
    public static async Task<HashSet<string>> GetTagIdsAsync(this AssetOperation operation, HashSet<string>? names)
    {
        var result = new HashSet<string>(names?.Count ?? 0);

        if (names != null)
        {
            var tagService = operation.Resolve<ITagService>();

            var normalized = await tagService.GetTagIdsAsync(operation.App.Id, TagGroups.Assets, names);

            result.AddRange(normalized.Values);
        }

        return result;
    }
}
