﻿// ==========================================================================
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
        private readonly IAssetEnricher assetEnricher = A.Fake<IAssetEnricher>();
        private readonly IAssetFileStore assetFileStore = A.Fake<IAssetFileStore>();
        private readonly IAssetMetadataSource assetMetadataSource = A.Fake<IAssetMetadataSource>();
        private readonly IAssetQueryService assetQuery = A.Fake<IAssetQueryService>();
        private readonly IContextProvider contextProvider = A.Fake<IContextProvider>();
        private readonly IGrainFactory grainFactory = A.Fake<IGrainFactory>();
        private readonly IServiceProvider serviceProvider = A.Fake<IServiceProvider>();
        private readonly ITagService tagService = A.Fake<ITagService>();
        private readonly Guid assetId = Guid.NewGuid();
        private readonly Stream stream = new MemoryStream();
        private readonly AssetDomainObjectGrain asset;
        private readonly AssetFile file;
        private readonly Context requestContext = Context.Anonymous();
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

            var assetDomainObject = new AssetDomainObject(Store, tagService, assetQuery, A.Dummy<ISemanticLog>());

            A.CallTo(() => serviceProvider.GetService(typeof(AssetDomainObject)))
                .Returns(assetDomainObject);

            asset = new AssetDomainObjectGrain(serviceProvider, null!);
            asset.ActivateAsync(Id).Wait();

            A.CallTo(() => contextProvider.Context)
                .Returns(requestContext);

            A.CallTo(() => assetEnricher.EnrichAsync(A<IAssetEntity>.Ignored, requestContext))
                .ReturnsLazily(() => SimpleMapper.Map(asset.Snapshot, new AssetEntity()));

            A.CallTo(() => assetQuery.QueryByHashAsync(A<Context>.That.Matches(x => x.ShouldEnrichAsset()), AppId, A<string>.Ignored))
                .Returns(new List<IEnrichedAssetEntity>());

            A.CallTo(() => grainFactory.GetGrain<IAssetGrain>(Id, null))
                .Returns(asset);

            sut = new AssetCommandMiddleware(grainFactory,
                assetEnricher,
                assetFileStore,
                assetQuery,
                contextProvider, new[] { assetMetadataSource });
        }

        [Fact]
        public async Task Should_not_invoke_enricher_for_other_result()
        {
            var command = CreateCommand(new MyCommand());
            var context = CreateContextForCommand(command);

            context.Complete(12);

            await sut.HandleAsync(context);

            A.CallTo(() => assetEnricher.EnrichAsync(A<IEnrichedAssetEntity>.Ignored, requestContext))
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

            A.CallTo(() => assetEnricher.EnrichAsync(A<IEnrichedAssetEntity>.Ignored, requestContext))
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

            A.CallTo(() => assetEnricher.EnrichAsync(result, requestContext))
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

            AssertAssetHasBeenUploaded(0);
            AssertMetadataEnriched();
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

            AssertAssetHasBeenUploaded(1);
            AssertMetadataEnriched();
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

        private void AssertAssetHasBeenUploaded(long version)
        {
            A.CallTo(() => assetFileStore.UploadAsync(A<string>.Ignored, A<HasherStream>.Ignored, CancellationToken.None))
                .MustHaveHappened();
            A.CallTo(() => assetFileStore.CopyAsync(A<string>.Ignored, Id, version, CancellationToken.None))
                .MustHaveHappened();
            A.CallTo(() => assetFileStore.DeleteAsync(A<string>.Ignored))
                .MustHaveHappened();
        }

        private void SetupSameHashAsset(string fileName, long fileSize, out IEnrichedAssetEntity duplicate)
        {
            duplicate = new AssetEntity
            {
                FileName = fileName,
                FileSize = fileSize
            };

            A.CallTo(() => assetQuery.QueryByHashAsync(A<Context>.That.Matches(x => !x.ShouldEnrichAsset()), A<Guid>.Ignored, A<string>.Ignored))
                .Returns(new List<IEnrichedAssetEntity> { duplicate });
        }

        private void AssertMetadataEnriched()
        {
            A.CallTo(() => assetMetadataSource.EnhanceAsync(A<UploadAssetCommand>.Ignored, A<HashSet<string>>.Ignored))
                .MustHaveHappened();
        }
    }
}
