// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Tags;
using Squidex.Infrastructure.States;

#pragma warning disable CS0649

namespace Squidex.Domain.Apps.Entities.Assets;

public partial class AssetUsageTracker : IDeleter
{
    private readonly IAssetLoader assetLoader;
    private readonly IAssetUsageTracker assetUsageTracker;
    private readonly ISnapshotStore<State> store;
    private readonly ITagService tagService;

    [CollectionName("Index_TagHistory")]
    public sealed class State
    {
        public HashSet<string>? Tags { get; set; }
    }

    public AssetUsageTracker(
        IAssetLoader assetLoader,
        IAssetUsageTracker assetUsageTracker,
        ITagService tagService,
        ISnapshotStore<State> store)
    {
        this.assetLoader = assetLoader;
        this.assetUsageTracker = assetUsageTracker;
        this.tagService = tagService;
        this.store = store;

        ClearCache();
    }

    Task IDeleter.DeleteAppAsync(App app,
        CancellationToken ct)
    {
        return assetUsageTracker.DeleteUsageAsync(app.Id, ct);
    }
}
