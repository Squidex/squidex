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
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Assets.DomainObject;

public class AssetDomainObjectTests : HandlerTestBase<AssetDomainObject.State>
{
    private readonly IAppEntity app;
    private readonly IAppProvider appProvider = A.Fake<IAppProvider>();
    private readonly IAssetQueryService assetQuery = A.Fake<IAssetQueryService>();
    private readonly IContentRepository contentRepository = A.Fake<IContentRepository>();
    private readonly IScriptEngine scriptEngine = A.Fake<IScriptEngine>();
    private readonly ITagService tagService = A.Fake<ITagService>();
    private readonly DomainId parentId = DomainId.NewGuid();
    private readonly DomainId assetId = DomainId.NewGuid();
    private readonly AssetFile file = new NoopAssetFile();
    private readonly AssetDomainObject sut;

    protected override DomainId Id
    {
        get => assetId;
    }

    public AssetDomainObjectTests()
    {
        app = Mocks.App(AppNamedId, Language.DE);

        var scripts = new AssetScripts
        {
            Annotate = "<annotate-script>",
            Create = "<create-script>",
            Delete = "<delete-script>",
            Move = "<move-script>",
            Update = "<update-script>"
        };

        A.CallTo(() => app.AssetScripts)
            .Returns(scripts);

        A.CallTo(() => appProvider.GetAppAsync(AppId, false, default))
            .Returns(app);

        A.CallTo(() => assetQuery.FindAssetFolderAsync(AppId, parentId, A<CancellationToken>._))
            .Returns(new List<IAssetFolderEntity> { A.Fake<IAssetFolderEntity>() });

        A.CallTo(() => tagService.GetTagIdsAsync(AppId, TagGroups.Assets, A<HashSet<string>>._, default))
            .ReturnsLazily(x => Task.FromResult(x.GetArgument<HashSet<string>>(2)?.ToDictionary(x => x) ?? new Dictionary<string, string>()));

        var log = A.Fake<ILogger<AssetDomainObject>>();

        var serviceProvider =
            new ServiceCollection()
                .AddSingleton(appProvider)
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

        var actual = await PublishAsync(command);

        actual.ShouldBeEquivalent(sut.Snapshot);

        Assert.Equal(0, sut.Snapshot.FileVersion);
        Assert.Equal(command.FileHash, sut.Snapshot.FileHash);

        LastEvents
            .ShouldHaveSameEvents(
                CreateAssetEvent(new AssetCreated
                {
                    FileName = file.FileName,
                    FileSize = file.FileSize,
                    FileHash = command.FileHash,
                    FileVersion = 0,
                    Metadata = new AssetMetadata(),
                    MimeType = file.MimeType,
                    Tags = new HashSet<string>(),
                    Slug = file.FileName.ToAssetSlug()
                })
            );

        A.CallTo(() => scriptEngine.ExecuteAsync(A<ScriptVars>._, "<create-script>", ScriptOptions(), default))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Create_should_recreate_deleted_content()
    {
        var command = new CreateAsset { File = file, FileHash = "NewHash" };

        await ExecuteCreateAsync();
        await ExecuteDeleteAsync();

        await PublishAsync(command);
    }

    [Fact]
    public async Task Create_should_recreate_permanently_deleted_content()
    {
        var command = new CreateAsset { File = file, FileHash = "NewHash" };

        await ExecuteCreateAsync();
        await ExecuteDeleteAsync(true);

        await PublishAsync(command);
    }

    [Fact]
    public async Task Upsert_should_create_events_and_set_intitial_state_if_not_found()
    {
        var command = new UpsertAsset { File = file, FileHash = "NewHash" };

        var actual = await PublishAsync(command);

        actual.ShouldBeEquivalent(sut.Snapshot);

        Assert.Equal(0, sut.Snapshot.FileVersion);
        Assert.Equal(command.FileHash, sut.Snapshot.FileHash);

        LastEvents
            .ShouldHaveSameEvents(
                CreateAssetEvent(new AssetCreated
                {
                    FileName = file.FileName,
                    FileSize = file.FileSize,
                    FileHash = command.FileHash,
                    FileVersion = 0,
                    Metadata = new AssetMetadata(),
                    MimeType = file.MimeType,
                    Tags = new HashSet<string>(),
                    Slug = file.FileName.ToAssetSlug()
                })
            );

        A.CallTo(() => scriptEngine.ExecuteAsync(A<ScriptVars>._, "<create-script>", ScriptOptions(), default))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Upsert_should_create_events_and_update_file_state_if_found()
    {
        var command = new UpsertAsset { File = file, FileHash = "NewHash" };

        await ExecuteCreateAsync();

        var actual = await PublishAsync(command);

        actual.ShouldBeEquivalent(sut.Snapshot);

        Assert.Equal(1, sut.Snapshot.FileVersion);
        Assert.Equal(command.FileHash, sut.Snapshot.FileHash);

        LastEvents
            .ShouldHaveSameEvents(
                CreateAssetEvent(new AssetUpdated
                {
                    FileSize = file.FileSize,
                    FileHash = command.FileHash,
                    FileVersion = 1,
                    Metadata = new AssetMetadata(),
                    MimeType = file.MimeType
                })
            );

        A.CallTo(() => scriptEngine.ExecuteAsync(A<ScriptVars>._, "<update-script>", ScriptOptions(), default))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Update_should_create_events_and_update_file_state()
    {
        var command = new UpdateAsset { File = file, FileHash = "NewHash" };

        await ExecuteCreateAsync();

        var actual = await PublishAsync(command);

        actual.ShouldBeEquivalent(sut.Snapshot);

        Assert.Equal(1, sut.Snapshot.FileVersion);
        Assert.Equal(command.FileHash, sut.Snapshot.FileHash);

        LastEvents
            .ShouldHaveSameEvents(
                CreateAssetEvent(new AssetUpdated
                {
                    FileSize = file.FileSize,
                    FileHash = command.FileHash,
                    FileVersion = 1,
                    Metadata = new AssetMetadata(),
                    MimeType = file.MimeType
                })
            );

        A.CallTo(() => scriptEngine.ExecuteAsync(A<ScriptVars>._, "<update-script>", ScriptOptions(), default))
            .MustHaveHappened();
    }

    [Fact]
    public async Task AnnotateName_should_create_events_and_update_file_name()
    {
        var command = new AnnotateAsset { FileName = "My New Image.png" };

        await ExecuteCreateAsync();

        var actual = await PublishIdempotentAsync(command);

        actual.ShouldBeEquivalent(sut.Snapshot);

        Assert.Equal(command.FileName, sut.Snapshot.FileName);

        LastEvents
            .ShouldHaveSameEvents(
                CreateAssetEvent(new AssetAnnotated { FileName = command.FileName })
            );

        A.CallTo(() => scriptEngine.ExecuteAsync(A<ScriptVars>._, "<annotate-script>", ScriptOptions(), default))
            .MustHaveHappened();
    }

    [Fact]
    public async Task AnnotateSlug_should_create_events_and_update_slug()
    {
        var command = new AnnotateAsset { Slug = "my-new-image.png" };

        await ExecuteCreateAsync();

        var actual = await PublishIdempotentAsync(command);

        actual.ShouldBeEquivalent(sut.Snapshot);

        Assert.Equal(command.Slug, sut.Snapshot.Slug);

        LastEvents
            .ShouldHaveSameEvents(
                CreateAssetEvent(new AssetAnnotated { Slug = command.Slug })
            );

        A.CallTo(() => scriptEngine.ExecuteAsync(A<ScriptVars>._, "<annotate-script>", ScriptOptions(), default))
            .MustHaveHappened();
    }

    [Fact]
    public async Task AnnotateProtected_should_create_events_and_update_protected_flag()
    {
        var command = new AnnotateAsset { IsProtected = true };

        await ExecuteCreateAsync();

        var actual = await PublishIdempotentAsync(command);

        actual.ShouldBeEquivalent(sut.Snapshot);

        Assert.Equal(command.IsProtected, sut.Snapshot.IsProtected);

        LastEvents
            .ShouldHaveSameEvents(
                CreateAssetEvent(new AssetAnnotated { IsProtected = command.IsProtected })
            );

        A.CallTo(() => scriptEngine.ExecuteAsync(A<ScriptVars>._, "<annotate-script>", ScriptOptions(), default))
            .MustHaveHappened();
    }

    [Fact]
    public async Task AnnotateMetadata_should_create_events_and_update_metadata()
    {
        var command = new AnnotateAsset { Metadata = new AssetMetadata().SetPixelWidth(800) };

        await ExecuteCreateAsync();

        var actual = await PublishIdempotentAsync(command);

        actual.ShouldBeEquivalent(sut.Snapshot);

        Assert.Equal(command.Metadata, sut.Snapshot.Metadata);

        LastEvents
            .ShouldHaveSameEvents(
                CreateAssetEvent(new AssetAnnotated { Metadata = command.Metadata })
            );

        A.CallTo(() => scriptEngine.ExecuteAsync(A<ScriptVars>._, "<anootate-script>", ScriptOptions(), default))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task AnnotateTags_should_create_events_and_update_tags()
    {
        var command = new AnnotateAsset { Tags = new HashSet<string> { "tag1" } };

        await ExecuteCreateAsync();

        var actual = await PublishIdempotentAsync(command);

        actual.ShouldBeEquivalent(sut.Snapshot);

        LastEvents
            .ShouldHaveSameEvents(
                CreateAssetEvent(new AssetAnnotated { Tags = new HashSet<string> { "tag1" } })
            );

        A.CallTo(() => scriptEngine.ExecuteAsync(A<ScriptVars>._, "<annotate-script>", ScriptOptions(), default))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Move_should_create_events_and_update_parent_id()
    {
        var command = new MoveAsset { ParentId = parentId };

        await ExecuteCreateAsync();

        var actual = await PublishIdempotentAsync(command);

        actual.ShouldBeEquivalent(sut.Snapshot);

        Assert.Equal(parentId, sut.Snapshot.ParentId);

        LastEvents
            .ShouldHaveSameEvents(
                CreateAssetEvent(new AssetMoved { ParentId = parentId })
            );

        A.CallTo(() => scriptEngine.ExecuteAsync(A<ScriptVars>._, "<move-script>", ScriptOptions(), default))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Delete_should_create_events_with_total_file_size_and_tags_and_update_deleted_flag()
    {
        var command = new DeleteAsset();

        await ExecuteCreateAsync();
        await ExecuteUpdateAsync();

        var actual = await PublishAsync(command);

        actual.ShouldBeEquivalent(None.Value);

        Assert.True(sut.Snapshot.IsDeleted);

        LastEvents
            .ShouldHaveSameEvents(
                CreateAssetEvent(new AssetDeleted { DeletedSize = 2048, OldTags = new HashSet<string>() })
            );

        A.CallTo(() => scriptEngine.ExecuteAsync(A<ScriptVars>._, "<delete-script>", ScriptOptions(), default))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Delete_should_not_create_events_if_permanent()
    {
        var command = new DeleteAsset { Permanent = true };

        await ExecuteCreateAsync();

        A.CallTo(() => contentRepository.HasReferrersAsync(AppId, Id, SearchScope.All, A<CancellationToken>._))
            .Returns(false);

        var actual = await PublishAsync(command);

        actual.ShouldBeEquivalent(None.Value);

        Assert.Equal(EtagVersion.Empty, sut.Snapshot.Version);
        Assert.Empty(LastEvents);

        A.CallTo(() => scriptEngine.ExecuteAsync(A<ScriptVars>._, "<delete-script>", ScriptOptions(), default))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Delete_should_throw_exception_if_referenced_by_other_item()
    {
        var command = new DeleteAsset { CheckReferrers = true };

        await ExecuteCreateAsync();

        A.CallTo(() => contentRepository.HasReferrersAsync(AppId, Id, SearchScope.All, A<CancellationToken>._))
            .Returns(true);

        await Assert.ThrowsAsync<DomainException>(() => PublishAsync(command));

        A.CallTo(() => scriptEngine.ExecuteAsync(A<ScriptVars>._, "<delete-script>", ScriptOptions(), default))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Delete_should_not_throw_exception_if_referenced_by_other_item_but_forced()
    {
        var command = new DeleteAsset();

        await ExecuteCreateAsync();

        A.CallTo(() => contentRepository.HasReferrersAsync(AppId, Id, SearchScope.All, A<CancellationToken>._))
            .Returns(true);

        await PublishAsync(command);

        A.CallTo(() => scriptEngine.ExecuteAsync(A<ScriptVars>._, "<delete-script>", ScriptOptions(), default))
            .MustHaveHappened();
    }

    private Task ExecuteCreateAsync()
    {
        return PublishAsync(new CreateAsset { File = file, FileHash = "123" });
    }

    private Task ExecuteUpdateAsync()
    {
        return PublishAsync(new UpdateAsset { File = file, FileHash = "456" });
    }

    private Task ExecuteDeleteAsync(bool permanent = false)
    {
        return PublishAsync(new DeleteAsset { Permanent = permanent });
    }

    private static ScriptOptions ScriptOptions()
    {
        return A<ScriptOptions>.That.Matches(x => x.CanDisallow && x.CanReject && x.AsContext);
    }

    private T CreateAssetEvent<T>(T @event) where T : AssetEvent
    {
        @event.AssetId = assetId;

        return CreateEvent(@event);
    }

    private T CreateAssetCommand<T>(T command) where T : AssetCommand
    {
        command.AssetId = assetId;

        return CreateCommand(command);
    }

    private Task<object> PublishIdempotentAsync(AssetCommand command)
    {
        return PublishIdempotentAsync(sut, CreateAssetCommand(command));
    }

    private async Task<object> PublishAsync(AssetCommand command)
    {
        var actual = await sut.ExecuteAsync(CreateAssetCommand(command), default);

        return actual.Payload;
    }
}
