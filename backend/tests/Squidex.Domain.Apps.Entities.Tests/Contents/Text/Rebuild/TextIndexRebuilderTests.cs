// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Entities.Contents.Text.State;

namespace Squidex.Domain.Apps.Entities.Contents.Text.Rebuild;

public class TextIndexRebuilderTests : TextIndexTestBase
{
    private readonly TextIndexRebuilder sut;

    public TextIndexRebuilderTests()
    {
        sut = CreateRebuilder();
    }

    [Fact]
    public async Task Should_rebuild_content_from_event_stream()
    {
        SetupStream(
            Created(TextData("field", "Version1"), 0),
            Published(1),
            Updated(TextData("field", "Version2"), 2));

        await sut.RebuildAsync(AppId.Id, [ContentId], CancellationToken);

        var upsert = Commands.OfType<UpsertIndexEntry>().Single();

        Assert.Equal("Version2", upsert.Texts?[InvariantPartitioning.Key]);
        Assert.Equal(0, upsert.Stage);
        Assert.True(upsert.ServeAll);
        Assert.True(upsert.ServePublished);

        // The entries exist already, therefore old geo objects and user infos must be replaced.
        Assert.False(upsert.IsNew);
    }

    [Fact]
    public async Task Should_delete_stages_that_do_not_exist_after_rebuild()
    {
        SetupStream(
            Created(TextData("field", "Version1"), 0));

        await sut.RebuildAsync(AppId.Id, [ContentId], CancellationToken);

        var delete = Commands.OfType<DeleteIndexEntry>().Single();

        Assert.Equal(1, delete.Stage);
    }

    [Fact]
    public async Task Should_store_version_after_rebuild()
    {
        SetupStream(
            Created(TextData("field", "Version1"), 0),
            Updated(TextData("field", "Version2"), 1));

        await sut.RebuildAsync(AppId.Id, [ContentId], CancellationToken);

        var state = await GetStateAsync();

        Assert.Equal(1, state?.Version);
        Assert.Equal(TextState.Stage0_Draft__Stage1_None, state?.State);
    }

    [Fact]
    public async Task Should_ignore_replayed_events_in_text_indexer_after_rebuild()
    {
        var created = Created(TextData("field", "Version1"), 0);
        var updated = Updated(TextData("field", "Version2"), 1);

        SetupStream(created, updated);

        await sut.RebuildAsync(AppId.Id, [ContentId], CancellationToken);

        Commands.Clear();

        // The text indexer is behind the event store and receives the events after the rebuild.
        await CreateProcess().On([created, updated]);

        Assert.Empty(Commands);
    }
}
