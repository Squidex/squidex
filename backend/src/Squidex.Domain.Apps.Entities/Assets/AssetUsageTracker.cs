﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Tags;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Billing;
using Squidex.Infrastructure.States;

#pragma warning disable CS0649

namespace Squidex.Domain.Apps.Entities.Assets
{
    public partial class AssetUsageTracker : IDeleter
    {
        private readonly IAssetLoader assetLoader;
        private readonly ISnapshotStore<State> store;
        private readonly ITagService tagService;
        private readonly IAppUsageGate appUsageGate;

        [CollectionName("Index_TagHistory")]
        public sealed class State
        {
            public HashSet<string>? Tags { get; set; }
        }

        public AssetUsageTracker(IAppUsageGate appUsageGate, IAssetLoader assetLoader, ITagService tagService,
            ISnapshotStore<State> store)
        {
            this.appUsageGate = appUsageGate;
            this.assetLoader = assetLoader;
            this.tagService = tagService;
            this.store = store;

            ClearCache();
        }

        Task IDeleter.DeleteAppAsync(IAppEntity app,
            CancellationToken ct)
        {
            return appUsageGate.DeleteAssetUsageAsync(app.Id, ct);
        }
    }
}
