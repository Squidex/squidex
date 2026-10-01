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
using Squidex.Infrastructure.States;
using IClock = NodaTime.IClock;

namespace Squidex.Domain.Apps.Entities.Contents.Text;

public abstract class TextIndexTestBase : GivenContext
{
    private readonly List<(StoredEvent Stored, Envelope<IEvent> Parsed)> storedEvents = [];
    private long lastVersion = -1;

    protected ITextIndex TextIndex { get; } = A.Fake<ITextIndex>();

    protected IEventStore EventStore { get; } = A.Fake<IEventStore>();

    protected IEventFormatter EventFormatter { get; } = A.Fake<IEventFormatter>();

    protected InMemoryTextIndexerState TextIndexerState { get; } = new InMemoryTextIndexerState();

    protected TextIndexRebuildCoordinator Coordinator { get; } = new TextIndexRebuildCoordinator();

    protected List<IndexCommand> Commands { get; } = [];

    protected DomainId ContentId { get; } = DomainId.NewGuid();

    protected TextIndexExtraction Extraction { get; }

    protected TextIndexTestBase()
    {
        A.CallTo(() => TextIndex.ExecuteAsync(A<IndexCommand[]>._, A<CancellationToken>._))
            .Invokes(x => Commands.AddRange(x.GetArgument<IndexCommand[]>(0)!));

        A.CallTo(() => EventStore.QueryAllAsync(A<StreamFilter>._, A<StreamPosition>._, A<int>._, A<CancellationToken>._))
            .ReturnsLazily(x => QueryEvents(x.GetArgument<StreamPosition>(1).Token));

        A.CallTo(() => EventFormatter.ParseIfKnown(A<StoredEvent>._))
            .ReturnsLazily(x => storedEvents.Find(e => e.Stored == x.GetArgument<StoredEvent>(0)).Parsed);

        var scriptEngine =
            new JintScriptEngine(new MemoryCache(Options.Create(new MemoryCacheOptions())),
                Options.Create(new JintScriptOptions
                {
                    TimeoutScript = TimeSpan.FromSeconds(2),
                    TimeoutExecution = TimeSpan.FromSeconds(10),
                }));

        Extraction = new TextIndexExtraction(AppProvider, TextExtractorTests.CreateExtractor(scriptEngine));
    }

    protected TextIndexingProcess CreateProcess()
    {
        return new TextIndexingProcess(TestUtils.DefaultSerializer, TextIndex, TextIndexerState, Extraction, Coordinator);
    }

    protected RebuildTextIndexJob CreateJob(TextIndexingProcess process)
    {
        return new RebuildTextIndexJob(AppProvider, EventStore, EventFormatter, process, Coordinator);
    }

    protected JobRunContext CreateRunContext()
    {
        var state = new SimpleState<JobsState>(A.Fake<IPersistenceFactory<JobsState>>(), GetType(), App.Id);

        var jobRequest = RebuildTextIndexJob.BuildRequest(User, App);

        return new JobRunContext(state, A.Fake<IClock>(), default)
        {
            Actor = User,
            Job = new Job { Id = DomainId.NewGuid(), Arguments = jobRequest.Arguments },
            OwnerId = App.Id,
        };
    }

    protected void StoreEvents(params Envelope<IEvent>[] events)
    {
        foreach (var @event in events)
        {
            var position = storedEvents.Count.ToString(CultureInfo.InvariantCulture);

            var stored = new StoredEvent("content", position, 0, new EventData("Type", [], "Payload"));

            storedEvents.Add((stored, @event));
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

    protected string? GetTitle()
    {
        return Commands.OfType<UpsertIndexEntry>().Last().Titles?[InvariantPartitioning.Key];
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

    private IAsyncEnumerable<StoredEvent> QueryEvents(string? position)
    {
        // The position is the index of the last event that has been read.
        var skip = position != null ? int.Parse(position, CultureInfo.InvariantCulture) + 1 : 0;

        return storedEvents.Skip(skip).Select(x => x.Stored).ToList().ToAsyncEnumerable();
    }

    private Envelope<IEvent> CreateEnvelope(ContentEvent @event, long? version, NamedId<DomainId>? appId = null)
    {
        @event.AppId = appId ?? AppId;
        @event.ContentId = ContentId;
        @event.SchemaId = SchemaId;

        // The indexer ignores events with versions that have already been indexed, therefore we need increasing versions.
        lastVersion = Math.Max(lastVersion, version ?? lastVersion + 1);

        return Envelope.Create<IEvent>(@event).SetEventStreamNumber(version ?? lastVersion);
    }
}
