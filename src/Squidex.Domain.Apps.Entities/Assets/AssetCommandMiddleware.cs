// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Domain.Apps.Entities.Tags;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Assets;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities.Assets
{
    public sealed class AssetCommandMiddleware : GrainCommandMiddleware<AssetCommand, IAssetGrain>
    {
        private readonly IAssetStore assetStore;
        private readonly IAssetThumbnailGenerator assetThumbnailGenerator;
        private readonly IEnumerable<ITagGenerator<CreateAsset>> tagGenerators;

        public AssetCommandMiddleware(
            IGrainFactory grainFactory,
            IAssetStore assetStore,
            IAssetThumbnailGenerator assetThumbnailGenerator,
            IEnumerable<ITagGenerator<CreateAsset>> tagGenerators)
            : base(grainFactory)
        {
            Guard.NotNull(assetStore, nameof(assetStore));
            Guard.NotNull(assetThumbnailGenerator, nameof(assetThumbnailGenerator));
            Guard.NotNull(tagGenerators, nameof(tagGenerators));

            this.assetStore = assetStore;
            this.assetThumbnailGenerator = assetThumbnailGenerator;

            this.tagGenerators = tagGenerators;
        }

        public override async Task HandleAsync(CommandContext context, Func<Task> next)
        {
            switch (context.Command)
            {
                case CreateAsset createAsset:
                    {
                        if (createAsset.Tags == null)
                        {
                            createAsset.Tags = new HashSet<string>();
                        }

                        createAsset.ImageInfo = await assetThumbnailGenerator.GetImageInfoAsync(createAsset.File.OpenRead());

                        foreach (var tagGenerator in tagGenerators)
                        {
                            tagGenerator.GenerateTags(createAsset, createAsset.Tags);
                        }

                        var originalTags = new HashSet<string>(createAsset.Tags);

                        await assetStore.UploadAsync(context.ContextId.ToString(), createAsset.File.OpenRead());
                        try
                        {
                            var result = await ExecuteCommandAsync(createAsset) as AssetSavedResult;

                            context.Complete(new AssetCreatedResult(createAsset.AssetId, originalTags, result.Version));

                            await assetStore.CopyAsync(context.ContextId.ToString(), createAsset.AssetId.ToString(), result.FileVersion, null);
                        }
                        finally
                        {
                            await assetStore.DeleteAsync(context.ContextId.ToString());
                        }

                        break;
                    }

                case UpdateAsset updateAsset:
                    {
                        updateAsset.ImageInfo = await assetThumbnailGenerator.GetImageInfoAsync(updateAsset.File.OpenRead());

                        await assetStore.UploadAsync(context.ContextId.ToString(), updateAsset.File.OpenRead());
                        try
                        {
                            var result = await ExecuteCommandAsync(updateAsset) as AssetSavedResult;

                            context.Complete(result);

                            await assetStore.CopyAsync(context.ContextId.ToString(), updateAsset.AssetId.ToString(), result.FileVersion, null);
                        }
                        finally
                        {
                            await assetStore.DeleteAsync(context.ContextId.ToString());
                        }

                        break;
                    }

                default:
                    await base.HandleAsync(context, next);
                    break;
            }
        }
    }
}
