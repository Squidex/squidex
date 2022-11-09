// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Tags;
using Squidex.Domain.Apps.Entities.Apps;

namespace Squidex.Domain.Apps.Entities.Assets;

public sealed class AssetTagsDeleter : IDeleter
{
    private readonly ITagService tagService;

    public AssetTagsDeleter(ITagService tagService)
    {
        this.tagService = tagService;
    }

    public Task DeleteAppAsync(IAppEntity app,
        CancellationToken ct)
    {
        return tagService.ClearAsync(app.Id, TagGroups.Assets, ct);
    }
}
