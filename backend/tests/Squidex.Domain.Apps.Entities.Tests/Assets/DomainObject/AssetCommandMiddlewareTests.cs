// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using Orleans;
using Squidex.Assets;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Orleans;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Assets.DomainObject
{
    public class AssetCommandMiddlewareTests : HandlerTestBase<AssetDomainObject.State>
    {
        private readonly IAssetEnricher assetEnricher = A.Fake<IAssetEnricher>();
        private readonly IAssetFileStore assetFileStore = A.Fake<IAssetFileStore>();
        private readonly IAssetMetadataSource assetMetadataSource = A.Fake<IAssetMetadataSource>();
        private readonly IAssetQueryService assetQuery = A.Fake<IAssetQueryService>();
        private readonly IContextProvider contextProvider = A.Fake<IContextProvider>();
        private readonly IGrainFactory grainFactory = A.Fake<IGrainFactory>();
        private readonly DomainId assetId = DomainId.NewGuid();
        private readonly AssetFile file = new NoopAssetFile();
        private readonly Context requestContext;
        private readonly AssetCommandMiddleware sut;

        public sealed class MyCommand : SquidexCommand
        {
        }

        protected override DomainId Id
        {
            get => DomainId.Combine(AppId, assetId);
        }

        public AssetCommandMiddlewareTests()
        {
            file = new NoopAssetFile();

            requestContext = Context.Anonymous(Mocks.App(AppNamedId));

            A.CallTo(() => contextProvider.Context)
                .Returns(requestContext);

            A.CallTo(() => assetQuery.FindByHashAsync(A<Context>._, A<string>._, A<string>._, A<long>._, A<CancellationToken>._))
                .Returns(Task.FromResult<IEnrichedAssetEntity?>(null));

            sut = new AssetCommandMiddleware(grainFactory,
                assetEnricher,
                assetFileStore,
                assetQuery,
                contextProvider, new[] { assetMetadataSource });
        }

        [Fact]
        public async Task Should_not_invoke_enricher_for_other_result()
        {
            await HandleAsync(new AnnotateAsset(), 12);

            A.CallTo(() => assetEnricher.EnrichAsync(A<IEnrichedAssetEntity>._, requestContext, A<CancellationToken>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_not_invoke_enricher_if_already_enriched()
        {
            var result = new AssetEntity();

            var context =
                await HandleAsync(new AnnotateAsset(),
                    result);

            A.CallTo(() => assetEnricher.EnrichAsync(A<IEnrichedAssetEntity>._, requestContext, A<CancellationToken>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_enrich_asset_result()
        {
            var result = A.Fake<IAssetEntity>();

            var enriched = new AssetEntity();

            A.CallTo(() => assetEnricher.EnrichAsync(result, requestContext, A<CancellationToken>._))
                .Returns(enriched);

            var context =
                await HandleAsync(new AnnotateAsset(),
                    result);

            Assert.Same(enriched, context.Result<IEnrichedAssetEntity>());
        }

        [Fact]
        public async Task Create_should_upload_file()
        {
            var result = CreateAsset();

            var context =
                await HandleAsync(new CreateAsset { File = file },
                    result);

            Assert.Same(result, context.Result<IEnrichedAssetEntity>());

            AssertAssetHasBeenUploaded(0);
            AssertMetadataEnriched();
        }

        [Fact]
        public async Task Create_should_calculate_hash()
        {
            var command = new CreateAsset { File = file };

            await HandleAsync(command, CreateAsset());

            Assert.True(command.FileHash.Length > 10);
        }

        [Fact]
        public async Task Create_should_not_return_duplicate_result_if_file_with_same_hash_found_but_duplicate_allowed()
        {
            var result = CreateAsset();

            SetupSameHashAsset(file.FileName, file.FileSize, out _);

            var context =
                await HandleAsync(new CreateAsset { File = file, Duplicate = true },
                    result);

            Assert.Same(result, context.Result<IEnrichedAssetEntity>());
        }

        [Fact]
        public async Task Create_should_return_duplicate_result_if_file_with_same_hash_found()
        {
            SetupSameHashAsset(file.FileName, file.FileSize, out var duplicate);

            var context =
                await HandleAsync(new CreateAsset { File = file },
                    CreateAsset());

            Assert.Same(duplicate, context.Result<AssetDuplicate>().Asset);
        }

        [Fact]
        public async Task Update_should_upload_file()
        {
            await HandleAsync(new UpdateAsset { File = file }, CreateAsset(1));

            AssertAssetHasBeenUploaded(1);
            AssertMetadataEnriched();
        }

        [Fact]
        public async Task Update_should_calculate_hash()
        {
            var command = new UpdateAsset { File = file };

            await HandleAsync(command, CreateAsset());

            Assert.True(command.FileHash.Length > 10);
        }

        [Fact]
        public async Task Upsert_should_upload_file()
        {
            await HandleAsync(new UpsertAsset { File = file }, CreateAsset(1));

            AssertAssetHasBeenUploaded(1);
            AssertMetadataEnriched();
        }

        [Fact]
        public async Task Upsert_should_calculate_hash()
        {
            var command = new UpsertAsset { File = file };

            await HandleAsync(command, CreateAsset());

            Assert.True(command.FileHash.Length > 10);
        }

        private void AssertAssetHasBeenUploaded(long fileVersion)
        {
            A.CallTo(() => assetFileStore.UploadAsync(A<string>._, A<HasherStream>._, default))
                .MustHaveHappened();
            A.CallTo(() => assetFileStore.CopyAsync(A<string>._, AppId, assetId, fileVersion, null, default))
                .MustHaveHappened();
            A.CallTo(() => assetFileStore.DeleteAsync(A<string>._, default))
                .MustHaveHappened();
        }

        private void SetupSameHashAsset(string fileName, long fileSize, out IEnrichedAssetEntity duplicate)
        {
            duplicate = new AssetEntity
            {
                FileName = fileName,
                FileSize = fileSize
            };

            A.CallTo(() => assetQuery.FindByHashAsync(requestContext, A<string>._, fileName, fileSize, A<CancellationToken>._))
                .Returns(duplicate);
        }

        private void AssertMetadataEnriched()
        {
            A.CallTo(() => assetMetadataSource.EnhanceAsync(A<UploadAssetCommand>._))
                .MustHaveHappened();
        }

        private Task<CommandContext> HandleAsync(AssetCommand command, object result)
        {
            command.AssetId = assetId;

            CreateCommand(command);

            var grain = A.Fake<IAssetGrain>();

            A.CallTo(() => grain.ExecuteAsync(A<J<CommandRequest>>._))
                .Returns(new CommandResult(command.AggregateId, 1, 0, result));

            A.CallTo(() => grainFactory.GetGrain<IAssetGrain>(command.AggregateId.ToString(), null))
                .Returns(grain);

            return HandleAsync(sut, command);
        }

        private IAssetEntity CreateAsset(long fileVersion = 0)
        {
            return new AssetEntity { AppId = AppNamedId, Id = assetId, FileVersion = fileVersion };
        }
    }
}
