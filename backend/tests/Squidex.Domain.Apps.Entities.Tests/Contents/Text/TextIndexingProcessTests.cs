// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Contents.Text.State;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.Text;

public class TextIndexingProcessTests : TextIndexTestBase
{
    private readonly TextIndexingProcess sut;

    public TextIndexingProcessTests()
    {
        sut = CreateProcess();
    }

    [Fact]
    public async Task Should_use_schema_to_extract_texts()
    {
        Schema = Schema
            .AddString(1, "title", Partitioning.Invariant)
            .AddString(2, "internal", Partitioning.Invariant,
                new StringFieldProperties { SearchMode = FieldSearchMode.Exclude });

        await sut.On([Created(TextData("title", "Hello").AddField("internal", new ContentFieldData().AddInvariant("Secret")))]);

        Assert.Equal("Hello", GetTitle());
        Assert.Null(GetText());
    }

    [Fact]
    public async Task Should_use_index_script_to_extract_texts()
    {
        Schema = Schema.SetScripts(new SchemaScripts
        {
            Index = "complete({ title: ctx.data.title.iv, body: 'World' });",
        });

        await sut.On([Created(TextData("title", "Hello"))]);

        Assert.Equal("Hello", GetTitle());
        Assert.Equal("World", GetText());
    }

    [Fact]
    public async Task Should_fallback_to_default_if_index_script_fails()
    {
        Schema = Schema.SetScripts(new SchemaScripts
        {
            Index = "throw 'Error';",
        });

        await sut.On([Created(TextData("field", "Hello"))]);

        Assert.Equal("Hello", GetText());
    }

    [Fact]
    public async Task Should_store_version_of_last_event()
    {
        await sut.On([Created(TextData("field", "Hello"), 0), Updated(TextData("field", "World"), 1)]);

        var state = await GetStateAsync();

        Assert.Equal(1, state?.Version);
    }

    [Fact]
    public async Task Should_skip_events_that_have_already_been_indexed_by_rebuild()
    {
        await TextIndexerState.SetAsync([new TextContentState { UniqueContentId = UniqueId(), State = TextState.Stage0_Draft__Stage1_None, Version = 5 }]);

        await sut.On([Updated(TextData("field", "Old"), 4), Updated(TextData("field", "Current"), 5)]);

        Assert.Empty(Commands);
    }

    [Fact]
    public async Task Should_handle_events_after_version_of_rebuild()
    {
        await TextIndexerState.SetAsync([new TextContentState { UniqueContentId = UniqueId(), State = TextState.Stage0_Draft__Stage1_None, Version = 5 }]);

        await sut.On([Updated(TextData("field", "Old"), 5), Updated(TextData("field", "New"), 6)]);

        Assert.Equal("New", GetText());
    }

    [Fact]
    public async Task Should_ignore_version_of_state_during_rebuild()
    {
        await TextIndexerState.SetAsync([new TextContentState { UniqueContentId = UniqueId(), State = TextState.Stage0_Draft__Stage1_None, Version = 5 }]);

        await sut.ApplyAsync([Created(TextData("field", "Hello"), 0)], true, CancellationToken);

        var state = await GetStateAsync();

        Assert.Equal("Hello", GetText());
        Assert.Equal(0, state?.Version);
    }

    [Fact]
    public async Task Should_only_index_final_version_of_content_in_batch()
    {
        await sut.On([Created(TextData("field", "Version1"), 0), Updated(TextData("field", "Version2"), 1)]);

        Assert.Equal("Version2", GetText());
        Assert.Single(Commands.OfType<UpsertIndexEntry>());
    }

    [Fact]
    public async Task Should_skip_and_count_events_of_app_that_is_rebuilt()
    {
        await Coordinator.TakeOverAsync(AppId.Id, CancellationToken);

        await sut.On([Created(TextData("field", "Hello"), 0), Updated(TextData("field", "World"), 1)]);

        var skipped = await Coordinator.GetSkippedEventsAsync(AppId.Id, CancellationToken);

        Assert.Empty(Commands);
        Assert.Equal(2, skipped);
    }

    [Fact]
    public async Task Should_index_other_apps_while_app_is_rebuilt()
    {
        await Coordinator.TakeOverAsync(AppId.Id, CancellationToken);

        var otherApp = NamedId.Of(DomainId.NewGuid(), "other-app");

        A.CallTo(() => AppProvider.GetSchemaAsync(otherApp.Id, A<DomainId>._, A<bool>._, A<CancellationToken>._))
            .Returns(Task.FromResult<Schema?>(null));

        await sut.On([Created(TextData("field", "Skipped"), 0), Created(TextData("field", "Other"), 0, otherApp)]);

        Assert.Equal("Other", GetText());
        Assert.Single(Commands);
    }

    [Fact]
    public async Task Should_index_app_again_after_hand_back()
    {
        await Coordinator.TakeOverAsync(AppId.Id, CancellationToken);
        await Coordinator.TryHandBackAsync(AppId.Id, 0, CancellationToken);

        await sut.On([Created(TextData("field", "Hello"), 0)]);

        Assert.Equal("Hello", GetText());
    }

    [Fact]
    public async Task Should_index_app_again_after_release()
    {
        await Coordinator.TakeOverAsync(AppId.Id, CancellationToken);
        await Coordinator.ReleaseAsync(AppId.Id);

        await sut.On([Created(TextData("field", "Hello"), 0)]);

        Assert.Equal("Hello", GetText());
    }
}
