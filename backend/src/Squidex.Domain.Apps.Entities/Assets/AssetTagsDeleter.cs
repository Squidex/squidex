// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Tags;

namespace Squidex.Domain.Apps.Entities.Assets;

public sealed class AssetTagsDeleter(ITagService tagService) : IDeleter
{
    public Task DeleteAppAsync(App app,
        CancellationToken ct)
    {
        return tagService.ClearAsync(app.Id, TagGroups.Assets, ct);
    }
}
