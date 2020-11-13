// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Orleans;
using Squidex.Assets;
using Squidex.Domain.Apps.Core.Tags;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Domain.Apps.Entities.Assets.State;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Reflection;
using Squidex.Log;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Assets
{
    public class AssetCommandMiddlewareTests : HandlerTestBase<AssetState>
    {
        private readonly IAssetEnricher assetEnricher = A.Fake<IAssetEnricher>();
        private readonly IAssetFileStore assetFileStore = A.Fake<IAssetFileStore>();
        private readonly IAssetMetadataSource assetMetadataSource = A.Fake<IAssetMetadataSource>();
        private readonly IAssetQueryService assetQuery = A.Fake<IAssetQueryService>();
        private readonly IContentRepository contentRepository = A.Fake<IContentRepository>();
        private readonly IContextProvider contextProvider = A.Fake<IContextProvider>();
        private readonly IGrainFactory grainFactory = A.Fake<IGrainFactory>();
        private readonly IServiceProvider serviceProvider = A.Fake<IServiceProvider>();
        private readonly ITagService tagService = A.Fake<ITagService>();
        private readonly DomainId assetId = DomainId.NewGuid();
        private readonly AssetDomainObjectGrain asset;
        private readonly AssetFile file;
        private readonly Context requestContext = Context.Anonymous();
        private readonly AssetCommandMiddleware sut;

        public sealed class MyCommand : SquidexCommand
        {
        }

        protected override DomainId Id
        {
            get { return DomainId.Combine(AppId, assetId); }
        }

        public AssetCommandMiddlewareTests()
        {
            file = new NoopAssetFile();

            var assetDomainObject = new AssetDomainObject(Store, A.Dummy<ISemanticLog>(), tagService, assetQuery, contentRepository);

            A.CallTo(() => serviceProvider.GetService(typeof(AssetDomainObject)))
                .Returns(assetDomainObject);

            asset = new AssetDomainObjectGrain(serviceProvider, null!);
            asset.ActivateAsync(Id.ToString()).Wait();

            A.CallTo(() => contextProvider.Context)
                .Returns(requestContext);

            A.CallTo(() => assetEnricher.EnrichAsync(A<IAssetEntity>._, requestContext))
                .ReturnsLazily(() => SimpleMapper.Map(asset.Snapshot, new AssetEntity()));

            A.CallTo(() => assetQuery.FindByHashAsync(A<Context>._, A<string>._, A<string>._, A<long>._))
                .Returns(Task.FromResult<IEnrichedAssetEntity?>(null));

            A.CallTo(() => grainFactory.GetGrain<IAssetGrain>(Id.ToString(), null))
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

            A.CallTo(() => assetEnricher.EnrichAsync(A<IEnrichedAssetEntity>._, requestContext))
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

            A.CallTo(() => assetEnricher.EnrichAsync(A<IEnrichedAssetEntity>._, requestContext))
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
        public async Task Create_should_not_return_duplicate_result_if_file_with_same_hash_found_but_duplicate_allowed()
        {
            var command = CreateCommand(new CreateAsset { AssetId = assetId, File = file, Duplicate = true });
            var context = CreateContextForCommand(command);

            SetupSameHashAsset(file.FileName, file.FileSize, out _);

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
            var command = CreateCommand(new CreateAsset { AssetId = assetId, File = file });

            return asset.ExecuteAsync(CommandRequest.Create(command));
        }

        private void AssertAssetHasBeenUploaded(long version)
        {
            A.CallTo(() => assetFileStore.UploadAsync(A<string>._, A<HasherStream>._, CancellationToken.None))
                .MustHaveHappened();
            A.CallTo(() => assetFileStore.CopyAsync(A<string>._, AppId, assetId, version, CancellationToken.None))
                .MustHaveHappened();
            A.CallTo(() => assetFileStore.DeleteAsync(A<string>._))
                .MustHaveHappened();
        }

        private void SetupSameHashAsset(string fileName, long fileSize, out IEnrichedAssetEntity duplicate)
        {
            duplicate = new AssetEntity
            {
                FileName = fileName,
                FileSize = fileSize
            };

            A.CallTo(() => assetQuery.FindByHashAsync(requestContext, A<string>._, fileName, fileSize))
                .Returns(duplicate);
        }

        private void AssertMetadataEnriched()
        {
            A.CallTo(() => assetMetadataSource.EnhanceAsync(A<UploadAssetCommand>._, A<HashSet<string>>._))
                .MustHaveHappened();
        }
    }
}
