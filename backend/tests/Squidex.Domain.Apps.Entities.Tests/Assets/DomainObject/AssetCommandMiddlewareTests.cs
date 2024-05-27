// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Assets;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Domain.Apps.Entities.Assets.Queries;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities.Assets.DomainObject;

public class AssetCommandMiddlewareTests : HandlerTestBase<Asset>
{
    private readonly IDomainObjectCache domainObjectCache = A.Fake<IDomainObjectCache>();
    private readonly IDomainObjectFactory domainObjectFactory = A.Fake<IDomainObjectFactory>();
    private readonly IAssetEnricher assetEnricher = A.Fake<IAssetEnricher>();
    private readonly IAssetFile file = new NoopAssetFile();
    private readonly IAssetFileStore assetFileStore = A.Fake<IAssetFileStore>();
    private readonly IAssetMetadataSource assetMetadataSource = A.Fake<IAssetMetadataSource>();
    private readonly IAssetQueryService assetQuery = A.Fake<IAssetQueryService>();
    private readonly DomainId assetId = DomainId.NewGuid();
    private readonly AssetCommandMiddleware sut;

    public sealed class MyCommand : SquidexCommand
    {
    }

    protected override DomainId Id
    {
        get => DomainId.Combine(AppId.Id, assetId);
    }

    public AssetCommandMiddlewareTests()
    {
        A.CallTo(() => assetQuery.FindByHashAsync(A<Context>._, A<string>._, A<string>._, A<long>._, CancellationToken))
            .Returns(Task.FromResult<EnrichedAsset?>(null));

        sut = new AssetCommandMiddleware(
            domainObjectFactory,
            domainObjectCache,
            assetEnricher,
            assetFileStore,
            assetQuery,
            ApiContextProvider, new[] { assetMetadataSource });
    }

    [Fact]
    public async Task Should_not_invoke_enricher_for_other_result()
    {
        await HandleAsync(new AnnotateAsset(), 12);

        A.CallTo(() => assetEnricher.EnrichAsync(A<EnrichedAsset>._, ApiContext, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_not_invoke_enricher_if_already_enriched()
    {
        var actual = new Asset();

        var context =
            await HandleAsync(new AnnotateAsset(),
                actual);

        A.CallTo(() => assetEnricher.EnrichAsync(A<EnrichedAsset>._, ApiContext, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_enrich_asset_result()
    {
        var actual = new Asset();

        var enriched = CreateAsset();

        A.CallTo(() => assetEnricher.EnrichAsync(actual, ApiContext, CancellationToken))
            .Returns(enriched);

        var context =
            await HandleAsync(new AnnotateAsset(),
                actual);

        Assert.Same(enriched, context.Result<EnrichedAsset>());
    }

    [Fact]
    public async Task Create_should_upload_file()
    {
        var actual = CreateAsset().WithId(assetId);

        var context =
            await HandleAsync(new CreateAsset { File = file },
                actual);

        Assert.Same(actual, context.Result<EnrichedAsset>());

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
        var actual = CreateAsset().WithId(assetId);

        SetupSameHashAsset(file.FileName, file.FileSize, out var _);

        var context =
            await HandleAsync(new CreateAsset { File = file, Duplicate = true },
                actual);

        Assert.Same(actual, context.Result<EnrichedAsset>());
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
        var actual = CreateAsset().WithId(assetId) with { FileVersion = 1 };

        await HandleAsync(new UpdateAsset { File = file }, actual);

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
        var actual = CreateAsset().WithId(assetId) with { FileVersion = 1 };

        await HandleAsync(new UpsertAsset { File = file, Duplicate = false }, actual);

        AssertAssetHasBeenUploaded(1);
        AssertMetadataEnriched();
    }

    [Fact]
    public async Task Upsert_should_not_return_duplicate_result_if_file_with_same_hash_found_but_duplicate_allowed()
    {
        var actual = CreateAsset().WithId(assetId);

        SetupSameHashAsset(file.FileName, file.FileSize, out var _);

        var context =
            await HandleAsync(new UpsertAsset { File = file },
                actual);

        Assert.Same(actual, context.Result<EnrichedAsset>());
    }

    [Fact]
    public async Task Upsert_should_return_duplicate_result_if_file_with_same_hash_found()
    {
        SetupSameHashAsset(file.FileName, file.FileSize, out var duplicate);

        var context =
            await HandleAsync(new UpsertAsset { File = file, Duplicate = false },
                CreateAsset());

        Assert.Same(duplicate, context.Result<AssetDuplicate>().Asset);
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
        A.CallTo(() => assetFileStore.UploadAsync(A<string>._, A<HasherStream>._, CancellationToken))
            .MustHaveHappened();

        A.CallTo(() => assetFileStore.CopyAsync(A<string>._, AppId.Id, assetId, fileVersion, null, CancellationToken))
            .MustHaveHappened();

        A.CallTo(() => assetFileStore.DeleteAsync(A<string>._, CancellationToken))
            .MustHaveHappened();
    }

    private void SetupSameHashAsset(string fileName, long fileSize, out EnrichedAsset duplicate)
    {
        duplicate = CreateAsset() with
        {
            FileName = fileName,
            FileSize = fileSize
        };

        A.CallTo(() => assetQuery.FindByHashAsync(ApiContext, A<string>._, fileName, fileSize, CancellationToken))
            .Returns(duplicate);
    }

    private void AssertMetadataEnriched()
    {
        A.CallTo(() => assetMetadataSource.EnhanceAsync(A<UploadAssetCommand>._, CancellationToken))
            .MustHaveHappened();
    }

    private Task<CommandContext> HandleAsync(AssetCommand command, object actual)
    {
        command.AssetId = assetId;

        CreateCommand(command);

        var domainObject = A.Fake<AssetDomainObject>();

        A.CallTo(() => domainObject.ExecuteAsync(A<IAggregateCommand>._, CancellationToken))
            .Returns(new CommandResult(command.AggregateId, 1, 0, actual));

        A.CallTo(() => domainObjectFactory.Create<AssetDomainObject>(command.AggregateId))
            .Returns(domainObject);

        return HandleAsync(sut, command, CancellationToken);
    }
}
