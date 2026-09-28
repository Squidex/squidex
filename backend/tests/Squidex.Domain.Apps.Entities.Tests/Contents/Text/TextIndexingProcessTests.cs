// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.Contents.Text.State;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Events;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Entities.Contents.Text;

public class TextIndexingProcessTests : GivenContext
{
    private readonly ITextIndex textIndex = A.Fake<ITextIndex>();
    private readonly IEventStore eventStore = A.Fake<IEventStore>();
    private readonly IEventFormatter eventFormatter = A.Fake<IEventFormatter>();
    private readonly InMemoryTextIndexerState textIndexerState = new InMemoryTextIndexerState();
    private readonly List<IndexCommand> commands = [];
    private readonly DomainId contentId = DomainId.NewGuid();
    private readonly TextIndexingProcess sut;

    public TextIndexingProcessTests()
    {
        A.CallTo(() => textIndex.ExecuteAsync(A<IndexCommand[]>._, A<CancellationToken>._))
            .Invokes(x => commands.AddRange(x.GetArgument<IndexCommand[]>(0)!));

        var scriptEngine =
            new JintScriptEngine(new MemoryCache(Options.Create(new MemoryCacheOptions())),
                Options.Create(new JintScriptOptions
                {
                    TimeoutScript = TimeSpan.FromSeconds(2),
                    TimeoutExecution = TimeSpan.FromSeconds(10),
                }));

        sut = new TextIndexingProcess(TestUtils.DefaultSerializer, textIndex, textIndexerState,
            AppProvider, TextExtractorTests.CreateExtractor(scriptEngine), eventStore, eventFormatter);
    }

    [Fact]
    public async Task Should_use_schema_to_extract_texts()
    {
        Schema = Schema
            .AddString(1, "title", Partitioning.Invariant)
            .AddString(2, "internal", Partitioning.Invariant,
                new StringFieldProperties { SearchMode = FieldSearchMode.Exclude });

        await sut.On([Created(TextData("title", "Hello").AddField("internal", new ContentFieldData().AddInvariant("Secret")))]);

        Assert.Equal("Hello Hello Hello", GetText());
    }

    [Fact]
    public async Task Should_use_index_script_to_extract_texts()
    {
        Schema = Schema.SetScripts(new SchemaScripts
        {
            Index = "complete({ title: ctx.data.title.iv, body: 'World' });",
        });

        await sut.On([Created(TextData("title", "Hello"))]);

        Assert.Equal("Hello Hello Hello World", GetText());
    }

    [Fact]
    public async Task Should_fallback_to_default_if_index_script_returns_nothing()
    {
        Schema = Schema.SetScripts(new SchemaScripts
        {
            Index = "var x = 1;",
        });

        await sut.On([Created(TextData("field", "Hello"))]);

        Assert.Equal("Hello", GetText());
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
        await textIndexerState.SetAsync([new TextContentState { UniqueContentId = UniqueId(), State = TextState.Stage0_Draft__Stage1_None, Version = 5 }]);

        await sut.On([Updated(TextData("field", "Old"), 4), Updated(TextData("field", "Current"), 5)]);

        Assert.Empty(commands);
    }

    [Fact]
    public async Task Should_handle_events_after_version_of_rebuild()
    {
        await textIndexerState.SetAsync([new TextContentState { UniqueContentId = UniqueId(), State = TextState.Stage0_Draft__Stage1_None, Version = 5 }]);

        await sut.On([Updated(TextData("field", "Old"), 5), Updated(TextData("field", "New"), 6)]);

        Assert.Equal("New", GetText());
    }

    [Fact]
    public async Task Should_rebuild_content_from_event_stream()
    {
        SetupStream(
            Created(TextData("field", "Version1"), 0),
            Published(1),
            Updated(TextData("field", "Version2"), 2));

        await sut.RebuildAsync(AppId.Id, [contentId], CancellationToken);

        var upsert = commands.OfType<UpsertIndexEntry>().Single();

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

        await sut.RebuildAsync(AppId.Id, [contentId], CancellationToken);

        var delete = commands.OfType<DeleteIndexEntry>().Single();

        Assert.Equal(1, delete.Stage);
    }

    [Fact]
    public async Task Should_store_version_after_rebuild()
    {
        SetupStream(
            Created(TextData("field", "Version1"), 0),
            Updated(TextData("field", "Version2"), 1));

        await sut.RebuildAsync(AppId.Id, [contentId], CancellationToken);

        var state = await GetStateAsync();

        Assert.Equal(1, state?.Version);
        Assert.Equal(TextState.Stage0_Draft__Stage1_None, state?.State);
    }

    [Fact]
    public async Task Should_ignore_replayed_events_after_rebuild()
    {
        var created = Created(TextData("field", "Version1"), 0);
        var updated = Updated(TextData("field", "Version2"), 1);

        SetupStream(created, updated);

        await sut.RebuildAsync(AppId.Id, [contentId], CancellationToken);

        commands.Clear();

        // The event consumer is behind the event store and receives the events after the rebuild.
        await sut.On([created, updated]);

        Assert.Empty(commands);
    }

    [Fact]
    public async Task Should_include_events_that_have_been_added_during_rebuild()
    {
        SetupStream(
            Created(TextData("field", "Version1"), 0),
            Updated(TextData("field", "Version2"), 1));

        SetupStream(1, null,
            Updated(TextData("field", "Version3"), 2));

        await sut.RebuildAsync(AppId.Id, [contentId], CancellationToken);

        var state = await GetStateAsync();

        Assert.Equal("Version3", GetText());
        Assert.Equal(2, state?.Version);
    }

    [Fact]
    public async Task Should_not_block_indexing_of_other_apps_during_rebuild()
    {
        var barrier = new TaskCompletionSource();

        SetupStream(
            Created(TextData("field", "Version1"), 0));

        // Blocks the rebuild while it holds the lock of the app.
        SetupStream(0, barrier.Task);

        var rebuild = sut.RebuildAsync(AppId.Id, [contentId], CancellationToken);

        var otherApp = NamedId.Of(DomainId.NewGuid(), "other-app");

        A.CallTo(() => AppProvider.GetSchemaAsync(otherApp.Id, A<DomainId>._, A<bool>._, A<CancellationToken>._))
            .Returns(Task.FromResult<Schema?>(null));

        var otherAppIndexing = sut.On([Created(TextData("field", "Other"), 0, otherApp)]);
        var sameAppIndexing = sut.On([Created(TextData("field", "Same"), 0)]);

        await otherAppIndexing.WaitAsync(TimeSpan.FromSeconds(5));

        Assert.False(sameAppIndexing.IsCompleted);
        Assert.False(rebuild.IsCompleted);

        barrier.SetResult();

        await rebuild;
        await sameAppIndexing;
    }

    private void SetupStream(params Envelope<IEvent>[] events)
    {
        SetupStream(EtagVersion.Empty, null, events);
    }

    private void SetupStream(long afterVersion, Task? barrier, params Envelope<IEvent>[] events)
    {
        var streamName = $"content-{DomainId.Combine(AppId.Id, contentId)}";

        var storedEvents = events.Select(e =>
        {
            var version = e.Headers.EventStreamNumber();

            return new StoredEvent(streamName, version.ToString(CultureInfo.InvariantCulture), version, new EventData("Type", [], "Payload"));
        }).ToList();

        A.CallTo(() => eventStore.QueryStreamAsync(streamName, afterVersion, CancellationToken))
            .ReturnsLazily(async _ =>
            {
                if (barrier != null)
                {
                    await barrier;
                }

                return (IReadOnlyList<StoredEvent>)storedEvents;
            });

        for (var i = 0; i < events.Length; i++)
        {
            var storedEvent = storedEvents[i];

            A.CallTo(() => eventFormatter.ParseIfKnown(storedEvent))
                .Returns(events[i]);
        }
    }

    private async Task<TextContentState?> GetStateAsync()
    {
        var states = await textIndexerState.GetAsync([UniqueId()]);

        return states.GetValueOrDefault(UniqueId());
    }

    private string? GetText()
    {
        return commands.OfType<UpsertIndexEntry>().Last().Texts?[InvariantPartitioning.Key];
    }

    private UniqueContentId UniqueId()
    {
        return new UniqueContentId(AppId.Id, contentId);
    }

    private static ContentData TextData(string field, string text)
    {
        return new ContentData()
            .AddField(field,
                new ContentFieldData()
                    .AddInvariant(text));
    }

    private Envelope<IEvent> Created(ContentData data, long? version = null, NamedId<DomainId>? appId = null)
    {
        return CreateEnvelope(new ContentCreated { Data = data }, version, appId);
    }

    private Envelope<IEvent> Updated(ContentData data, long? version = null)
    {
        return CreateEnvelope(new ContentUpdated { Data = data }, version);
    }

    private Envelope<IEvent> Published(long? version = null)
    {
        return CreateEnvelope(new ContentStatusChanged { Status = Status.Published }, version);
    }

    private Envelope<IEvent> CreateEnvelope(ContentEvent @event, long? version, NamedId<DomainId>? appId = null)
    {
        @event.AppId = appId ?? AppId;
        @event.ContentId = contentId;
        @event.SchemaId = SchemaId;

        var envelope = Envelope.Create<IEvent>(@event);

        if (version != null)
        {
            envelope.SetEventStreamNumber(version.Value);
        }

        return envelope;
    }
}
