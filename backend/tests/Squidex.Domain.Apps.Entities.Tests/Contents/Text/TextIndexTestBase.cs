// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.Contents.Text.Rebuild;
using Squidex.Domain.Apps.Entities.Contents.Text.State;
using Squidex.Domain.Apps.Entities.Jobs;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Events;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Entities.Contents.Text;

public abstract class TextIndexTestBase : GivenContext
{
    protected ITextIndex TextIndex { get; } = A.Fake<ITextIndex>();

    protected IEventStore EventStore { get; } = A.Fake<IEventStore>();

    protected IEventFormatter EventFormatter { get; } = A.Fake<IEventFormatter>();

    protected IJobService JobService { get; } = A.Fake<IJobService>();

    protected InMemoryTextIndexerState TextIndexerState { get; } = new InMemoryTextIndexerState();

    protected List<IndexCommand> Commands { get; } = [];

    protected DomainId ContentId { get; } = DomainId.NewGuid();

    protected TextIndexRebuildRegistry Registry { get; } =
        new TextIndexRebuildRegistry(
            new InMemoryPersistenceFactory<TextIndexRebuildRequests>(),
            new InMemoryPersistenceFactory<TextIndexSkipList>());

    protected TextIndexRebuildCoordinator Coordinator { get; }

    protected TextIndexExtraction Extraction { get; }

    protected TextIndexTestBase()
    {
        A.CallTo(() => TextIndex.ExecuteAsync(A<IndexCommand[]>._, A<CancellationToken>._))
            .Invokes(x => Commands.AddRange(x.GetArgument<IndexCommand[]>(0)!));

        var scriptEngine =
            new JintScriptEngine(new MemoryCache(Options.Create(new MemoryCacheOptions())),
                Options.Create(new JintScriptOptions
                {
                    TimeoutScript = TimeSpan.FromSeconds(2),
                    TimeoutExecution = TimeSpan.FromSeconds(10),
                }));

        Extraction = new TextIndexExtraction(AppProvider, TextExtractorTests.CreateExtractor(scriptEngine));

        Coordinator = new TextIndexRebuildCoordinator(Registry, AppProvider, JobService, A.Fake<ILogger<TextIndexRebuildCoordinator>>());
    }

    protected TextIndexingProcess CreateProcess()
    {
        return new TextIndexingProcess(TestUtils.DefaultSerializer, TextIndex, TextIndexerState, Extraction, Coordinator);
    }

    protected TextIndexRebuilder CreateRebuilder()
    {
        return new TextIndexRebuilder(TestUtils.DefaultSerializer, TextIndex, TextIndexerState, Extraction, EventStore, EventFormatter);
    }

    protected void SetupStream(params Envelope<IEvent>[] events)
    {
        var streamName = $"content-{DomainId.Combine(AppId.Id, ContentId)}";

        var storedEvents = events.Select(e =>
        {
            var version = e.Headers.EventStreamNumber();

            return new StoredEvent(streamName, version.ToString(CultureInfo.InvariantCulture), version, new EventData("Type", [], "Payload"));
        }).ToList();

        A.CallTo(() => EventStore.QueryStreamAsync(streamName, EtagVersion.Empty, A<CancellationToken>._))
            .Returns(storedEvents);

        for (var i = 0; i < events.Length; i++)
        {
            var storedEvent = storedEvents[i];

            A.CallTo(() => EventFormatter.ParseIfKnown(storedEvent))
                .Returns(events[i]);
        }
    }

    protected async Task<TextContentState?> GetStateAsync()
    {
        var states = await TextIndexerState.GetAsync([UniqueId()]);

        return states.GetValueOrDefault(UniqueId());
    }

    protected string? GetText()
    {
        return Commands.OfType<UpsertIndexEntry>().Last().Texts?[InvariantPartitioning.Key];
    }

    protected UniqueContentId UniqueId()
    {
        return new UniqueContentId(AppId.Id, ContentId);
    }

    protected static ContentData TextData(string field, string text)
    {
        return new ContentData()
            .AddField(field,
                new ContentFieldData()
                    .AddInvariant(text));
    }

    protected Envelope<IEvent> Created(ContentData data, long? version = null, NamedId<DomainId>? appId = null)
    {
        return CreateEnvelope(new ContentCreated { Data = data }, version, appId);
    }

    protected Envelope<IEvent> Updated(ContentData data, long? version = null)
    {
        return CreateEnvelope(new ContentUpdated { Data = data }, version);
    }

    protected Envelope<IEvent> Published(long? version = null)
    {
        return CreateEnvelope(new ContentStatusChanged { Status = Status.Published }, version);
    }

    private Envelope<IEvent> CreateEnvelope(ContentEvent @event, long? version, NamedId<DomainId>? appId = null)
    {
        @event.AppId = appId ?? AppId;
        @event.ContentId = ContentId;
        @event.SchemaId = SchemaId;

        var envelope = Envelope.Create<IEvent>(@event);

        if (version != null)
        {
            envelope.SetEventStreamNumber(version.Value);
        }

        return envelope;
    }
}
