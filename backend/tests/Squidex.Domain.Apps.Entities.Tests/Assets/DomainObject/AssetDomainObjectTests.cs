// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Squidex.Assets;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Core.Tags;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities.Assets.DomainObject;

public class AssetDomainObjectTests : HandlerTestBase<Asset>
{
    private readonly IAssetFile file = new NoopAssetFile();
    private readonly IAssetQueryService assetQuery = A.Fake<IAssetQueryService>();
    private readonly IContentRepository contentRepository = A.Fake<IContentRepository>();
    private readonly IScriptEngine scriptEngine = A.Fake<IScriptEngine>();
    private readonly ITagService tagService = A.Fake<ITagService>();
    private readonly DomainId parentId = DomainId.NewGuid();
    private readonly DomainId assetId = DomainId.NewGuid();
    private readonly AssetDomainObject sut;

    protected override DomainId Id
    {
        get => assetId;
    }

    public AssetDomainObjectTests()
    {
        App = App with
        {
            AssetScripts = new AssetScripts
            {
                Annotate = "<annotate-script>",
                Create = "<create-script>",
                Delete = "<delete-script>",
                Move = "<move-script>",
                Update = "<update-script>"
            }
        };

        A.CallTo(() => assetQuery.FindAssetFolderAsync(AppId.Id, parentId, A<CancellationToken>._))
            .Returns(new List<AssetFolder> { A.Fake<AssetFolder>() });

        A.CallTo(() => tagService.GetTagIdsAsync(AppId.Id, TagGroups.Assets, A<HashSet<string>>._, default))
            .ReturnsLazily(x => Task.FromResult(x.GetArgument<HashSet<string>>(2)?.ToDictionary(x => x) ?? []));

        var log = A.Fake<ILogger<AssetDomainObject>>();

        var serviceProvider =
            new ServiceCollection()
                .AddSingleton(AppProvider)
                .AddSingleton(assetQuery)
                .AddSingleton(contentRepository)
                .AddSingleton(log)
                .AddSingleton(scriptEngine)
                .AddSingleton(tagService)
                .BuildServiceProvider();

#pragma warning disable MA0056 // Do not call overridable members in constructor
        sut = new AssetDomainObject(Id, PersistenceFactory, log, serviceProvider);
#pragma warning restore MA0056 // Do not call overridable members in constructor
    }

    [Fact]
    public async Task Command_should_throw_exception_if_asset_is_deleted()
    {
        await ExecuteCreateAsync();
        await ExecuteDeleteAsync();

        await Assert.ThrowsAsync<DomainObjectDeletedException>(ExecuteUpdateAsync);
    }

    [Fact]
    public async Task Create_should_create_events_and_set_intitial_state()
    {
        var command = new CreateAsset { File = file, FileHash = "NewHash" };

        var actual = await PublishAsync(sut, command);

        await VerifySutAsync(actual);

        A.CallTo(() => scriptEngine.ExecuteAsync(A<ScriptVars>._, "<create-script>", ScriptOptions(), CancellationToken))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Create_should_recreate_deleted_content()
    {
        var command = new CreateAsset { File = file, FileHash = "NewHash" };

        await ExecuteCreateAsync();
        await ExecuteDeleteAsync();

        await PublishAsync(sut, command);
    }

    [Fact]
    public async Task Create_should_recreate_permanently_deleted_content()
    {
        var command = new CreateAsset { File = file, FileHash = "NewHash" };

        await ExecuteCreateAsync();
        await ExecuteDeleteAsync(true);

        await PublishAsync(sut, command);
    }

    [Fact]
    public async Task Upsert_should_create_events_and_set_intitial_state_if_not_found()
    {
        var command = new UpsertAsset { File = file, FileHash = "NewHash" };

        var actual = await PublishAsync(sut, command);

        await VerifySutAsync(actual);

        A.CallTo(() => scriptEngine.ExecuteAsync(A<ScriptVars>._, "<create-script>", ScriptOptions(), CancellationToken))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Upsert_should_create_events_and_update_file_state_if_found()
    {
        var command = new UpsertAsset { File = file, FileHash = "NewHash" };

        await ExecuteCreateAsync();

        var actual = await PublishAsync(sut, command);

        await VerifySutAsync(actual);

        A.CallTo(() => scriptEngine.ExecuteAsync(A<ScriptVars>._, "<update-script>", ScriptOptions(), CancellationToken))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Update_should_create_events_and_update_file_state()
    {
        var command = new UpdateAsset { File = file, FileHash = "NewHash" };

        await ExecuteCreateAsync();

        var actual = await PublishAsync(sut, command);

        await VerifySutAsync(actual);

        A.CallTo(() => scriptEngine.ExecuteAsync(A<ScriptVars>._, "<update-script>", ScriptOptions(), CancellationToken))
            .MustHaveHappened();
    }

    [Fact]
    public async Task AnnotateName_should_create_events_and_update_file_name()
    {
        var command = new AnnotateAsset { FileName = "My New Image.png" };

        await ExecuteCreateAsync();

        var actual = await PublishIdempotentAsync(sut, command);

        await VerifySutAsync(actual);

        A.CallTo(() => scriptEngine.ExecuteAsync(A<ScriptVars>._, "<annotate-script>", ScriptOptions(), CancellationToken))
            .MustHaveHappened();
    }

    [Fact]
    public async Task AnnotateSlug_should_create_events_and_update_slug()
    {
        var command = new AnnotateAsset { Slug = "my-new-image.png" };

        await ExecuteCreateAsync();

        var actual = await PublishIdempotentAsync(sut, command);

        await VerifySutAsync(actual);

        A.CallTo(() => scriptEngine.ExecuteAsync(A<ScriptVars>._, "<annotate-script>", ScriptOptions(), CancellationToken))
            .MustHaveHappened();
    }

    [Fact]
    public async Task AnnotateProtected_should_create_events_and_update_protected_flag()
    {
        var command = new AnnotateAsset { IsProtected = true };

        await ExecuteCreateAsync();

        var actual = await PublishIdempotentAsync(sut, command);

        await VerifySutAsync(actual);

        A.CallTo(() => scriptEngine.ExecuteAsync(A<ScriptVars>._, "<annotate-script>", ScriptOptions(), CancellationToken))
            .MustHaveHappened();
    }

    [Fact]
    public async Task AnnotateMetadata_should_create_events_and_update_metadata()
    {
        var command = new AnnotateAsset { Metadata = new AssetMetadata { [KnownMetadataKeys.PixelWidth] = 800 } };

        await ExecuteCreateAsync();

        var actual = await PublishIdempotentAsync(sut, command);

        await VerifySutAsync(actual);

        A.CallTo(() => scriptEngine.ExecuteAsync(A<ScriptVars>._, "<anootate-script>", ScriptOptions(), CancellationToken))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task AnnotateTags_should_create_events_and_update_tags()
    {
        var command = new AnnotateAsset { Tags = ["tag1"] };

        await ExecuteCreateAsync();

        var actual = await PublishIdempotentAsync(sut, command);

        await VerifySutAsync(actual);

        A.CallTo(() => scriptEngine.ExecuteAsync(A<ScriptVars>._, "<annotate-script>", ScriptOptions(), CancellationToken))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Move_should_create_events_and_update_parent_id()
    {
        var command = new MoveAsset { ParentId = parentId };

        await ExecuteCreateAsync();

        var actual = await PublishIdempotentAsync(sut, command);

        await VerifySutAsync(actual);

        A.CallTo(() => scriptEngine.ExecuteAsync(A<ScriptVars>._, "<move-script>", ScriptOptions(), CancellationToken))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Delete_should_create_events_with_total_file_size_and_tags_and_update_deleted_flag()
    {
        var command = new DeleteAsset();

        await ExecuteCreateAsync();
        await ExecuteUpdateAsync();

        var actual = await PublishAsync(sut, command);

        await VerifySutAsync(actual, None.Value);

        A.CallTo(() => scriptEngine.ExecuteAsync(A<ScriptVars>._, "<delete-script>", ScriptOptions(), CancellationToken))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Delete_should_not_create_events_if_permanent()
    {
        var command = new DeleteAsset { Permanent = true };

        await ExecuteCreateAsync();

        A.CallTo(() => contentRepository.HasReferrersAsync(App, Id, SearchScope.All, A<CancellationToken>._))
            .Returns(false);

        await PublishAsync(sut, command);

        Assert.Empty(LastEvents);

        A.CallTo(() => scriptEngine.ExecuteAsync(A<ScriptVars>._, "<delete-script>", ScriptOptions(), CancellationToken))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Delete_should_throw_exception_if_referenced_by_other_item()
    {
        var command = new DeleteAsset { CheckReferrers = true };

        await ExecuteCreateAsync();

        A.CallTo(() => contentRepository.HasReferrersAsync(App, Id, SearchScope.All, A<CancellationToken>._))
            .Returns(true);

        await Assert.ThrowsAsync<DomainException>(() => PublishAsync(sut, command));

        A.CallTo(() => scriptEngine.ExecuteAsync(A<ScriptVars>._, "<delete-script>", ScriptOptions(), CancellationToken))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Delete_should_not_throw_exception_if_referenced_by_other_item_but_forced()
    {
        var command = new DeleteAsset();

        await ExecuteCreateAsync();

        A.CallTo(() => contentRepository.HasReferrersAsync(App, Id, SearchScope.All, A<CancellationToken>._))
            .Returns(true);

        await PublishAsync(sut, command);

        A.CallTo(() => scriptEngine.ExecuteAsync(A<ScriptVars>._, "<delete-script>", ScriptOptions(), CancellationToken))
            .MustHaveHappened();
    }

    private Task ExecuteCreateAsync()
    {
        return PublishAsync(sut, new CreateAsset { File = file, FileHash = "123" });
    }

    private Task ExecuteUpdateAsync()
    {
        return PublishAsync(sut, new UpdateAsset { File = file, FileHash = "456" });
    }

    private Task ExecuteDeleteAsync(bool permanent = false)
    {
        return PublishAsync(sut, new DeleteAsset { Permanent = permanent });
    }

    private static ScriptOptions ScriptOptions()
    {
        return A<ScriptOptions>.That.Matches(x => x.CanDisallow && x.CanReject && x.AsContext);
    }

    protected override IAggregateCommand CreateCommand(IAggregateCommand command)
    {
        ((AssetCommand)command).AssetId = assetId;

        return base.CreateCommand(command);
    }

    private async Task VerifySutAsync(object? actual, object? expected = null)
    {
        if (expected == null)
        {
            actual.Should().BeEquivalentTo(sut.Snapshot, o => o.IncludingProperties());
        }
        else
        {
            actual.Should().BeEquivalentTo(expected);
        }

        Assert.Equal(AppId, sut.Snapshot.AppId);

        await Verify(new { sut, events = LastEvents });
    }
}
