// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using Orleans;
using Squidex.Domain.Apps.Core.Tags;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Domain.Apps.Entities.Assets.State;
using Squidex.Domain.Apps.Entities.Tags;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure.Assets;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Log;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Assets
{
    public class AssetCommandMiddlewareTests : HandlerTestBase<AssetGrain, AssetState>
    {
        private readonly IAssetThumbnailGenerator assetThumbnailGenerator = A.Fake<IAssetThumbnailGenerator>();
        private readonly IAssetStore assetStore = A.Fake<IAssetStore>();
        private readonly ITagService tagService = A.Fake<ITagService>();
        private readonly ITagGenerator<CreateAsset> tagGenerator = A.Fake<ITagGenerator<CreateAsset>>();
        private readonly IGrainFactory grainFactory = A.Fake<IGrainFactory>();
        private readonly Guid assetId = Guid.NewGuid();
        private readonly Stream stream = new MemoryStream();
        private readonly ImageInfo image = new ImageInfo(2048, 2048);
        private readonly AssetGrain asset;
        private readonly AssetFile file;
        private readonly AssetCommandMiddleware sut;

        protected override Guid Id
        {
            get { return assetId; }
        }

        public AssetCommandMiddlewareTests()
        {
            file = new AssetFile("my-image.png", "image/png", 1024, () => stream);

            asset = new AssetGrain(Store, tagService, A.Dummy<ISemanticLog>());
            asset.OnActivateAsync(Id).Wait();

            A.CallTo(() => tagService.NormalizeTagsAsync(AppId, TagGroups.Assets, A<HashSet<string>>.Ignored, A<HashSet<string>>.Ignored))
                .Returns(new Dictionary<string, string>());

            A.CallTo(() => grainFactory.GetGrain<IAssetGrain>(Id, null))
                .Returns(asset);

            sut = new AssetCommandMiddleware(grainFactory, assetStore, assetThumbnailGenerator, new[] { tagGenerator });
        }

        [Fact]
        public async Task Create_should_create_domain_object()
        {
            var command = new CreateAsset { AssetId = assetId, File = file };
            var context = CreateContextForCommand(command);

            A.CallTo(() => tagGenerator.GenerateTags(command, A<HashSet<string>>.Ignored))
                .Invokes(new Action<CreateAsset, HashSet<string>>((c, tags) =>
                {
                    tags.Add("tag1");
                    tags.Add("tag2");
                }));

            SetupImageInfo();

            await sut.HandleAsync(context);

            var result = context.Result<AssetCreatedResult>();

            Assert.Equal(assetId, result.Id);
            Assert.Contains("tag1", result.Tags);
            Assert.Contains("tag2", result.Tags);

            AssertAssetHasBeenUploaded(0, context.ContextId);
            AssertAssetImageChecked();
        }

        [Fact]
        public async Task Update_should_update_domain_object()
        {
            var context = CreateContextForCommand(new UpdateAsset { AssetId = assetId, File = file });

            SetupImageInfo();

            await ExecuteCreateAsync();

            await sut.HandleAsync(context);

            AssertAssetHasBeenUploaded(1, context.ContextId);
            AssertAssetImageChecked();
        }

        private Task ExecuteCreateAsync()
        {
            return asset.ExecuteAsync(CreateCommand(new CreateAsset { AssetId = Id, File = file }));
        }

        private void AssertAssetHasBeenUploaded(long version, Guid commitId)
        {
            A.CallTo(() => assetStore.UploadAsync(commitId.ToString(), stream, CancellationToken.None))
                .MustHaveHappened();
            A.CallTo(() => assetStore.CopyAsync(commitId.ToString(), assetId.ToString(), version, null, CancellationToken.None))
                .MustHaveHappened();
            A.CallTo(() => assetStore.DeleteAsync(commitId.ToString()))
                .MustHaveHappened();
        }

        private void SetupImageInfo()
        {
            A.CallTo(() => assetThumbnailGenerator.GetImageInfoAsync(stream))
                .Returns(image);
        }

        private void AssertAssetImageChecked()
        {
            A.CallTo(() => assetThumbnailGenerator.GetImageInfoAsync(stream))
                .MustHaveHappened();
        }
    }
}
