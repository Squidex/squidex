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
using FluentAssertions;
using Orleans;
using Squidex.Domain.Apps.Core.Tags;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Domain.Apps.Entities.Assets.State;
using Squidex.Domain.Apps.Entities.Tags;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure.Assets;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Reflection;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Assets
{
    public class AssetCommandMiddlewareTests : HandlerTestBase<AssetState>
    {
        private readonly IAssetQueryService assetQuery = A.Fake<IAssetQueryService>();
        private readonly IAssetEnricher assetEnricher = A.Fake<IAssetEnricher>();
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

        public sealed class MyCommand : SquidexCommand
        {
        }

        protected override Guid Id
        {
            get { return assetId; }
        }

        public AssetCommandMiddlewareTests()
        {
            file = new AssetFile("my-image.png", "image/png", 1024, () => stream);

            asset = new AssetGrain(Store, tagService, A.Dummy<ISemanticLog>());
            asset.ActivateAsync(Id).Wait();

            A.CallTo(() => assetEnricher.EnrichAsync(A<IAssetEntity>.Ignored))
                .ReturnsLazily(() => SimpleMapper.Map(asset.Snapshot, new AssetEntity()));

            A.CallTo(() => assetQuery.QueryByHashAsync(AppId, A<string>.Ignored))
                .Returns(new List<IEnrichedAssetEntity>());

            A.CallTo(() => grainFactory.GetGrain<IAssetGrain>(Id, null))
                .Returns(asset);

            A.CallTo(() => assetThumbnailGenerator.GetImageInfoAsync(stream))
                .Returns(image);

            sut = new AssetCommandMiddleware(grainFactory,
                assetEnricher,
                assetQuery,
                assetStore,
                assetThumbnailGenerator, new[] { tagGenerator });
        }

        [Fact]
        public async Task Should_not_invoke_enricher_for_other_result()
        {
            var command = CreateCommand(new MyCommand());
            var context = CreateContextForCommand(command);

            context.Complete(12);

            await sut.HandleAsync(context);

            A.CallTo(() => assetEnricher.EnrichAsync(A<IEnrichedAssetEntity>.Ignored))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_not_invoke_enricher_if_already_enriched()
        {
            var result = new AssetEntity();

            var command = CreateCommand(new MyCommand());
            var context = CreateContextForCommand(command);

            context.Complete(result);

            await sut.HandleAsync(context);

            Assert.Same(result, context.Result<IEnrichedAssetEntity>());

            A.CallTo(() => assetEnricher.EnrichAsync(A<IEnrichedAssetEntity>.Ignored))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_enrich_asset_result()
        {
            var result = A.Fake<IAssetEntity>();

            var command = CreateCommand(new MyCommand());
            var context = CreateContextForCommand(command);

            context.Complete(result);

            var enriched = new AssetEntity();

            A.CallTo(() => assetEnricher.EnrichAsync(result))
                .Returns(enriched);

            await sut.HandleAsync(context);

            Assert.Same(enriched, context.Result<IEnrichedAssetEntity>());
        }

        [Fact]
        public async Task Create_should_create_domain_object()
        {
            var command = CreateCommand(new CreateAsset { AssetId = assetId, File = file });
            var context = CreateContextForCommand(command);

            await sut.HandleAsync(context);

            var result = context.Result<AssetCreatedResult>();

            result.Asset.Should().BeEquivalentTo(asset.Snapshot, x => x.ExcludingMissingMembers());
        }

        [Fact]
        public async Task Create_should_calculate_hash()
        {
            var command = CreateCommand(new CreateAsset { AssetId = assetId, File = file });
            var context = CreateContextForCommand(command);

            await sut.HandleAsync(context);

            Assert.True(command.FileHash.Length > 10);
        }

        [Fact]
        public async Task Create_should_return_duplicate_result_if_file_with_same_hash_found()
        {
            var command = CreateCommand(new CreateAsset { AssetId = assetId, File = file });
            var context = CreateContextForCommand(command);

            SetupSameHashAsset(file.FileName, file.FileSize, out _);

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

            await sut.HandleAsync(context);

            var result = context.Result<AssetCreatedResult>();

            Assert.False(result.IsDuplicate);
        }

        [Fact]
        public async Task Create_should_pass_through_duplicate()
        {
            var command = CreateCommand(new CreateAsset { AssetId = assetId, File = file });
            var context = CreateContextForCommand(command);

            SetupSameHashAsset(file.FileName, file.FileSize, out var duplicate);

            await sut.HandleAsync(context);

            var result = context.Result<AssetCreatedResult>();

            Assert.True(result.IsDuplicate);

            result.Should().BeEquivalentTo(duplicate, x => x.ExcludingMissingMembers());
        }

        [Fact]
        public async Task Create_should_not_return_duplicate_result_if_file_with_same_hash_but_other_size_found()
        {
            var command = CreateCommand(new CreateAsset { AssetId = assetId, File = file });
            var context = CreateContextForCommand(command);

            SetupSameHashAsset(file.FileName, 12345, out _);

            await sut.HandleAsync(context);

            Assert.False(context.Result<AssetCreatedResult>().IsDuplicate);
        }

        [Fact]
        public async Task Update_should_update_domain_object()
        {
            var command = CreateCommand(new UpdateAsset { AssetId = assetId, File = file });
            var context = CreateContextForCommand(command);

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

            await ExecuteCreateAsync();

            await sut.HandleAsync(context);

            Assert.True(command.FileHash.Length > 10);
        }

        [Fact]
        public async Task Update_should_enrich_asset()
        {
            var command = CreateCommand(new UpdateAsset { AssetId = assetId, File = file });
            var context = CreateContextForCommand(command);

            await ExecuteCreateAsync();

            await sut.HandleAsync(context);

            var result = context.Result<IEnrichedAssetEntity>();

            result.Should().BeEquivalentTo(asset.Snapshot, x => x.ExcludingMissingMembers());
        }

        [Fact]
        public async Task AnnotateAsset_should_enrich_asset()
        {
            var command = CreateCommand(new AnnotateAsset { AssetId = assetId, FileName = "newName" });
            var context = CreateContextForCommand(command);

            await ExecuteCreateAsync();

            await sut.HandleAsync(context);

            var result = context.Result<IEnrichedAssetEntity>();

            result.Should().BeEquivalentTo(asset.Snapshot, x => x.ExcludingMissingMembers());
        }

        private Task ExecuteCreateAsync()
        {
            return asset.ExecuteAsync(CreateCommand(new CreateAsset { AssetId = Id, File = file }));
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

        private void SetupSameHashAsset(string fileName, long fileSize, out IEnrichedAssetEntity duplicate)
        {
            duplicate = new AssetEntity
            {
                FileName = fileName,
                FileSize = fileSize
            };

            A.CallTo(() => assetQuery.QueryByHashAsync(A<Guid>.Ignored, A<string>.Ignored))
                .Returns(new List<IEnrichedAssetEntity> { duplicate });
        }

        private void AssertAssetImageChecked()
        {
            A.CallTo(() => assetThumbnailGenerator.GetImageInfoAsync(stream))
                .MustHaveHappened();
        }
    }
}
