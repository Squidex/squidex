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
    public class AssetCommandMiddlewareTests : HandlerTestBase<AssetState>
    {
        private readonly IAssetQueryService assetQuery = A.Fake<IAssetQueryService>();
        private readonly IAssetThumbnailGenerator assetThumbnailGenerator = A.Fake<IAssetThumbnailGenerator>();
        private readonly IAssetStore assetStore = A.Fake<MemoryAssetStore>();
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
            asset.ActivateAsync(Id).Wait();

            A.CallTo(() => assetQuery.QueryByHashAsync(AppId, A<string>.Ignored))
                .Returns(new List<IAssetEntity>());

            A.CallTo(() => tagService.DenormalizeTagsAsync(AppId, TagGroups.Assets, A<HashSet<string>>.Ignored))
                .Returns(new Dictionary<string, string>
                {
                    ["1"] = "foundTag1",
                    ["2"] = "foundTag2"
                });

            A.CallTo(() => grainFactory.GetGrain<IAssetGrain>(Id, null))
                .Returns(asset);

            sut = new AssetCommandMiddleware(grainFactory, assetQuery, assetStore, assetThumbnailGenerator, new[] { tagGenerator }, tagService);
        }

        [Fact]
        public async Task Create_should_create_domain_object()
        {
            var command = CreateCommand(new CreateAsset { AssetId = assetId, File = file });
            var context = CreateContextForCommand(command);

            SetupTags(command);
            SetupImageInfo();

            await sut.HandleAsync(context);

            var result = context.Result<AssetCreatedResult>();

            Assert.Equal(assetId, result.Asset.Id);
            Assert.Contains("tag1", command.Tags);
            Assert.Contains("tag2", command.Tags);

            Assert.Equal(new HashSet<string> { "tag1", "tag2" }, result.Tags);

            AssertAssetHasBeenUploaded(0, context.ContextId);
            AssertAssetImageChecked();
        }

        [Fact]
        public async Task Create_should_calculate_hash()
        {
            var command = CreateCommand(new CreateAsset { AssetId = assetId, File = file });
            var context = CreateContextForCommand(command);

            SetupImageInfo();

            await sut.HandleAsync(context);

            Assert.True(command.FileHash.Length > 10);
        }

        [Fact]
        public async Task Create_should_return_duplicate_result_if_file_with_same_hash_found()
        {
            var command = CreateCommand(new CreateAsset { AssetId = assetId, File = file });
            var context = CreateContextForCommand(command);

            SetupSameHashAsset(file.FileName, file.FileSize, out _);
            SetupImageInfo();

            await sut.HandleAsync(context);

            var result = context.Result<AssetCreatedResult>();

            Assert.True(result.IsDuplicate);
        }

        [Fact]
        public async Task Create_should_not_return_duplicate_result_if_file_with_same_hash_but_other_name_found()
        {
            var command = CreateCommand(new CreateAsset { AssetId = assetId, File = file });
            var context = CreateContextForCommand(command);

            SetupSameHashAsset("other-name", file.FileSize, out _);
            SetupImageInfo();

            await sut.HandleAsync(context);

            var result = context.Result<AssetCreatedResult>();

            Assert.False(result.IsDuplicate);
        }

        [Fact]
        public async Task Create_should_resolve_tag_names_for_duplicate()
        {
            var command = CreateCommand(new CreateAsset { AssetId = assetId, File = file });
            var context = CreateContextForCommand(command);

            SetupSameHashAsset(file.FileName, file.FileSize, out _);
            SetupImageInfo();

            await sut.HandleAsync(context);

            var result = context.Result<AssetCreatedResult>();

            Assert.Equal(new HashSet<string> { "foundTag1", "foundTag2" }, result.Tags);
        }

        [Fact]
        public async Task Create_should_not_return_duplicate_result_if_file_with_same_hash_but_other_size_found()
        {
            var command = CreateCommand(new CreateAsset { AssetId = assetId, File = file });
            var context = CreateContextForCommand(command);

            SetupSameHashAsset(file.FileName, 12345, out _);
            SetupImageInfo();

            await sut.HandleAsync(context);

            Assert.False(context.Result<AssetCreatedResult>().IsDuplicate);
        }

        [Fact]
        public async Task Update_should_update_domain_object()
        {
            var command = CreateCommand(new UpdateAsset { AssetId = assetId, File = file });
            var context = CreateContextForCommand(command);

            SetupImageInfo();

            await ExecuteCreateAsync();

            await sut.HandleAsync(context);

            AssertAssetHasBeenUploaded(1, context.ContextId);
            AssertAssetImageChecked();
        }

        [Fact]
        public async Task Update_should_calculate_hash()
        {
            var command = CreateCommand(new UpdateAsset { AssetId = assetId, File = file });
            var context = CreateContextForCommand(command);

            SetupImageInfo();

            await ExecuteCreateAsync();

            await sut.HandleAsync(context);

            Assert.True(command.FileHash.Length > 10);
        }

        [Fact]
        public async Task Update_should_resolve_tags()
        {
            var command = CreateCommand(new UpdateAsset { AssetId = assetId, File = file });
            var context = CreateContextForCommand(command);

            SetupImageInfo();

            await ExecuteCreateAsync();

            await sut.HandleAsync(context);

            var result = context.Result<AssetResult>();

            Assert.Equal(new HashSet<string> { "foundTag1", "foundTag2" }, result.Tags);
        }

        [Fact]
        public async Task AnnotateAsset_should_resolve_tags()
        {
            var command = CreateCommand(new AnnotateAsset { AssetId = assetId, FileName = "newName" });
            var context = CreateContextForCommand(command);

            SetupImageInfo();

            await ExecuteCreateAsync();

            await sut.HandleAsync(context);

            var result = context.Result<AssetResult>();

            Assert.Equal(new HashSet<string> { "foundTag1", "foundTag2" }, result.Tags);
        }

        private Task ExecuteCreateAsync()
        {
            return asset.ExecuteAsync(CreateCommand(new CreateAsset { AssetId = Id, File = file }));
        }

        private void SetupTags(CreateAsset command)
        {
            A.CallTo(() => tagGenerator.GenerateTags(command, A<HashSet<string>>.Ignored))
                .Invokes(new Action<CreateAsset, HashSet<string>>((c, tags) =>
                {
                    tags.Add("tag1");
                    tags.Add("tag2");
                }));
        }

        private void AssertAssetHasBeenUploaded(long version, Guid commitId)
        {
            var fileName = AssetStoreExtensions.GetFileName(assetId.ToString(), version);

            A.CallTo(() => assetStore.UploadAsync(commitId.ToString(), A<HasherStream>.Ignored, false, CancellationToken.None))
                .MustHaveHappened();
            A.CallTo(() => assetStore.CopyAsync(commitId.ToString(), fileName, CancellationToken.None))
                .MustHaveHappened();
            A.CallTo(() => assetStore.DeleteAsync(commitId.ToString()))
                .MustHaveHappened();
        }

        private void SetupSameHashAsset(string fileName, long fileSize, out IAssetEntity existing)
        {
            var temp = existing = A.Fake<IAssetEntity>();

            A.CallTo(() => temp.FileName).Returns(fileName);
            A.CallTo(() => temp.FileSize).Returns(fileSize);

            A.CallTo(() => assetQuery.QueryByHashAsync(A<Guid>.Ignored, A<string>.Ignored))
                .Returns(new List<IAssetEntity> { existing });
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
