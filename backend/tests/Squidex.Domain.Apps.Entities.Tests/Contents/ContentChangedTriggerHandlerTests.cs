// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Entities.Contents;

public class ContentChangedTriggerHandlerTests : GivenContext
{
    private readonly IScriptEngine scriptEngine = A.Fake<IScriptEngine>();
    private readonly IContentLoader contentLoader = A.Fake<IContentLoader>();
    private readonly IContentRepository contentRepository = A.Fake<IContentRepository>();
    private readonly NamedId<DomainId> schemaMatching = NamedId.Of(DomainId.NewGuid(), "my-schema1");
    private readonly NamedId<DomainId> schemaNotMatching = NamedId.Of(DomainId.NewGuid(), "my-schema2");
    private readonly IRuleTriggerHandler sut;

    public static readonly TheoryData<ContentEvent, EnrichedContentEventType> TestEvents = new TheoryData<ContentEvent, EnrichedContentEventType>()
    {
        { TestUtils.CreateEvent<ContentCreated>(), EnrichedContentEventType.Created },
        { TestUtils.CreateEvent<ContentUpdated>(), EnrichedContentEventType.Updated },
        { TestUtils.CreateEvent<ContentDeleted>(), EnrichedContentEventType.Deleted },
        { TestUtils.CreateEvent<ContentStatusChanged>(x => x.Change = StatusChange.Change), EnrichedContentEventType.StatusChanged },
        { TestUtils.CreateEvent<ContentStatusChanged>(x => x.Change = StatusChange.Published), EnrichedContentEventType.Published },
        { TestUtils.CreateEvent<ContentStatusChanged>(x => x.Change = StatusChange.Unpublished), EnrichedContentEventType.Unpublished }
    };

    public ContentChangedTriggerHandlerTests()
    {
        A.CallTo(() => scriptEngine.Evaluate(A<ScriptVars>._, "true", default))
            .Returns(true);

        A.CallTo(() => scriptEngine.Evaluate(A<ScriptVars>._, "false", default))
            .Returns(false);

        sut = new ContentChangedTriggerHandler(scriptEngine, contentLoader, contentRepository);
    }

    [Fact]
    public void Should_return_true_if_asking_for_snapshot_support()
    {
        Assert.True(sut.CanCreateSnapshotEvents);
    }

    [Fact]
    public void Should_handle_content_event()
    {
        Assert.True(sut.Handles(new ContentCreated()));
    }

    [Fact]
    public void Should_not_handle_other_event()
    {
        Assert.False(sut.Handles(new AssetMoved()));
    }

    [Fact]
    public void Should_calculate_name_for_created()
    {
        var @event = new ContentCreated { SchemaId = schemaMatching };

        Assert.Equal("MySchema1Created", sut.GetName(@event));
    }

    [Fact]
    public void Should_calculate_name_for_deleted()
    {
        var @event = new ContentDeleted { SchemaId = schemaMatching };

        Assert.Equal("MySchema1Deleted", sut.GetName(@event));
    }

    [Fact]
    public void Should_calculate_name_for_updated()
    {
        var @event = new ContentUpdated { SchemaId = schemaMatching };

        Assert.Equal("MySchema1Updated", sut.GetName(@event));
    }

    [Fact]
    public void Should_calculate_name_for_published()
    {
        var @event = new ContentStatusChanged { SchemaId = schemaMatching, Change = StatusChange.Published };

        Assert.Equal("MySchema1Published", sut.GetName(@event));
    }

    [Fact]
    public void Should_calculate_name_for_unpublished()
    {
        var @event = new ContentStatusChanged { SchemaId = schemaMatching, Change = StatusChange.Unpublished };

        Assert.Equal("MySchema1Unpublished", sut.GetName(@event));
    }

    [Fact]
    public void Should_calculate_name_for_status_change()
    {
        var @event = new ContentStatusChanged { SchemaId = schemaMatching, Change = StatusChange.Change };

        Assert.Equal("MySchema1StatusChanged", sut.GetName(@event));
    }

    [Fact]
    public async Task Should_create_events_from_snapshots()
    {
        var ctx = Context();

        A.CallTo(() => contentRepository.StreamAll(AppId.Id, null, SearchScope.All, CancellationToken))
            .Returns(new List<Content>
            {
                CreateContent() with { SchemaId = schemaMatching },
                CreateContent() with { SchemaId = schemaNotMatching }
            }.ToAsyncEnumerable());

        var actual = await sut.CreateSnapshotEventsAsync(ctx, CancellationToken).ToListAsync(CancellationToken);

        var typed = actual.OfType<EnrichedContentEvent>().ToList();

        Assert.Equal(2, typed.Count);
        Assert.Equal(2, typed.Count(x => x.Type == EnrichedContentEventType.Created));

        Assert.Equal("ContentQueried(MySchema1)", typed[0].Name);
        Assert.Equal("ContentQueried(MySchema2)", typed[1].Name);
    }

    [Fact]
    public async Task Should_create_events_from_snapshots_with_schema_ids()
    {
        var trigger = new ContentChangedTriggerV2
        {
            Schemas = ReadonlyList.Create(
                new SchemaCondition
                {
                    SchemaId = schemaMatching.Id
                })
        };

        var ctx = Context(trigger);

        A.CallTo(() => contentRepository.StreamAll(AppId.Id, A<HashSet<DomainId>>.That.Is(schemaMatching.Id), SearchScope.All, CancellationToken))
            .Returns(new List<Content>
            {
                CreateContent() with { SchemaId = schemaMatching },
                CreateContent() with { SchemaId = schemaMatching }
            }.ToAsyncEnumerable());

        var actual = await sut.CreateSnapshotEventsAsync(ctx, CancellationToken).ToListAsync(CancellationToken);

        var typed = actual.OfType<EnrichedContentEvent>().ToList();

        Assert.Equal(2, typed.Count);
        Assert.Equal(2, typed.Count(x => x.Type == EnrichedContentEventType.Created));
    }

    [Theory]
    [MemberData(nameof(TestEvents))]
    public async Task Should_create_enriched_events(ContentEvent @event, EnrichedContentEventType type)
    {
        var ctx = Context().ToRulesContext();

        @event.AppId = AppId;
        @event.SchemaId = schemaMatching;

        var envelope = Envelope.Create<AppEvent>(@event).SetEventStreamNumber(12);

        A.CallTo(() => contentLoader.GetAsync(AppId.Id, @event.ContentId, 12, CancellationToken))
            .Returns(SimpleMapper.Map(@event, new Content()));

        var actuals = await sut.CreateEnrichedEventsAsync(envelope, ctx, CancellationToken).ToListAsync(CancellationToken);
        var actual = (EnrichedContentEvent)actuals.Single();

        Assert.Equal(type, actual!.Type);
        Assert.Equal(@event.Actor, actual.Actor);
        Assert.Equal(@event.AppId, actual.AppId);
        Assert.Equal(@event.AppId.Id, actual.AppId.Id);
        Assert.Equal(@event.SchemaId, actual.SchemaId);
        Assert.Equal(@event.SchemaId.Id, actual.SchemaId.Id);
    }

    [Fact]
    public async Task Should_enrich_with_old_data_if_updated()
    {
        var ctx = Context().ToRulesContext();

        var @event = new ContentUpdated { AppId = AppId, ContentId = DomainId.NewGuid(), SchemaId = schemaMatching };
        var envelope = Envelope.Create<AppEvent>(@event).SetEventStreamNumber(12);

        var dataNow = new ContentData();
        var dataOld = new ContentData();

        A.CallTo(() => contentLoader.GetAsync(AppId.Id, @event.ContentId, 12, CancellationToken))
            .Returns(new Content { AppId = AppId, SchemaId = schemaMatching, Version = 12, Data = dataNow, Id = @event.ContentId });

        A.CallTo(() => contentLoader.GetAsync(AppId.Id, @event.ContentId, 11, CancellationToken))
            .Returns(new Content { AppId = AppId, SchemaId = schemaMatching, Version = 11, Data = dataOld });

        var actuals = await sut.CreateEnrichedEventsAsync(envelope, ctx, CancellationToken).ToListAsync(CancellationToken);
        var actual = actuals.Single() as EnrichedContentEvent;

        Assert.Same(dataNow, actual!.Data);
        Assert.Same(dataOld, actual!.DataOld);
    }

    [Fact]
    public async Task Should_query_references_if_filters_match()
    {
        var ctx = ReferencingContext(100, true);

        var @event = new ContentUpdated { AppId = AppId, ContentId = DomainId.NewGuid(), SchemaId = schemaMatching };
        var envelope = Envelope.Create<AppEvent>(@event).SetEventStreamNumber(12);

        SetupData(@event, 12);

        A.CallTo(() => contentRepository.StreamReferencing(AppId.Id, @event.ContentId, 100, SearchScope.All, CancellationToken))
            .Returns(new List<Content>
            {
                new Content { SchemaId = schemaMatching },
                new Content { SchemaId = schemaMatching }
            }.ToAsyncEnumerable());

        var actual = await sut.CreateEnrichedEventsAsync(envelope, ctx, CancellationToken).ToListAsync(CancellationToken);

        Assert.Equal(2, actual.OfType<EnrichedContentEvent>().Count(x => x.Type == EnrichedContentEventType.ReferenceUpdated));
    }

    [Fact]
    public async Task Should_not_query_references_if_filter_does_not_match()
    {
        var ctx = ReferencingContext(100, true, schemaNotMatching);

        var @event = new ContentUpdated { AppId = AppId, ContentId = DomainId.NewGuid(), SchemaId = schemaMatching };
        var envelope = Envelope.Create<AppEvent>(@event).SetEventStreamNumber(12);

        SetupData(@event, 12);

        await sut.CreateEnrichedEventsAsync(envelope, ctx, CancellationToken).ToListAsync(CancellationToken);

        A.CallTo(contentRepository)
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_not_query_references_if_extra_events_not_enabled()
    {
        var ctx = ReferencingContext(100, false, schemaMatching);

        var @event = new ContentUpdated { AppId = AppId, ContentId = DomainId.NewGuid(), SchemaId = schemaMatching };
        var envelope = Envelope.Create<AppEvent>(@event).SetEventStreamNumber(12);

        SetupData(@event, 12);

        await sut.CreateEnrichedEventsAsync(envelope, ctx, CancellationToken).ToListAsync(CancellationToken);

        A.CallTo(contentRepository)
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_not_query_references_if_number_of_events_is_null()
    {
        var ctx = ReferencingContext(null, false, schemaMatching);

        var @event = new ContentUpdated { AppId = AppId, ContentId = DomainId.NewGuid(), SchemaId = schemaMatching };
        var envelope = Envelope.Create<AppEvent>(@event).SetEventStreamNumber(12);

        SetupData(@event, 12);

        await sut.CreateEnrichedEventsAsync(envelope, ctx, CancellationToken).ToListAsync(CancellationToken);

        A.CallTo(contentRepository)
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_not_query_references_if_number_of_events_is_zero()
    {
        var ctx = ReferencingContext(null, false, schemaMatching);

        var @event = new ContentUpdated { AppId = AppId, ContentId = DomainId.NewGuid(), SchemaId = schemaMatching };
        var envelope = Envelope.Create<AppEvent>(@event).SetEventStreamNumber(12);

        SetupData(@event, 12);

        await sut.CreateEnrichedEventsAsync(envelope, ctx, CancellationToken).ToListAsync(CancellationToken);

        A.CallTo(contentRepository)
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_not_query_references_if_created_event()
    {
        var ctx = ReferencingContext(100, true, schemaMatching);

        var @event = new ContentCreated { AppId = AppId, ContentId = DomainId.NewGuid(), SchemaId = schemaMatching };
        var envelope = Envelope.Create<AppEvent>(@event).SetEventStreamNumber(12);

        SetupData(@event, 12);

        await sut.CreateEnrichedEventsAsync(envelope, ctx, CancellationToken).ToListAsync(CancellationToken);

        A.CallTo(contentRepository)
            .MustNotHaveHappened();
    }

    [Fact]
    public void Should_not_trigger_precheck_if_trigger_contains_no_schemas()
    {
        TestForTrigger(handleAll: false, schemaId: null, condition: null, action: ctx =>
        {
            var @event = new ContentCreated { SchemaId = schemaMatching };

            var actual = sut.Trigger(Envelope.Create<AppEvent>(@event), ctx.Rule.Trigger);

            Assert.False(actual);
        });
    }

    [Fact]
    public void Should_trigger_precheck_if_handling_all_events()
    {
        TestForTrigger(handleAll: true, schemaId: schemaMatching, condition: null, action: ctx =>
        {
            var @event = new ContentCreated { SchemaId = schemaMatching };

            var actual = sut.Trigger(Envelope.Create<AppEvent>(@event), ctx.Rule.Trigger);

            Assert.True(actual);
        });
    }

    [Fact]
    public void Should_trigger_precheck_if_condition_is_empty()
    {
        TestForTrigger(handleAll: false, schemaId: schemaMatching, condition: string.Empty, action: ctx =>
        {
            var @event = new ContentCreated { SchemaId = schemaMatching };

            var actual = sut.Trigger(Envelope.Create<AppEvent>(@event), ctx.Rule.Trigger);

            Assert.True(actual);
        });
    }

    [Fact]
    public void Should_not_trigger_precheck_if_schema_id_does_not_match()
    {
        TestForTrigger(handleAll: false, schemaId: schemaNotMatching, condition: null, action: ctx =>
        {
            var @event = new ContentCreated { SchemaId = schemaMatching };

            var actual = sut.Trigger(Envelope.Create<AppEvent>(@event), ctx.Rule.Trigger);

            Assert.False(actual);
        });
    }

    [Fact]
    public void Should_not_trigger_check_if_trigger_contains_no_schemas()
    {
        TestForTrigger(handleAll: false, schemaId: null, condition: null, action: ctx =>
        {
            var @event = new EnrichedContentEvent { SchemaId = schemaMatching };

            var actual = sut.Trigger(@event, ctx.Rule.Trigger);

            Assert.False(actual);
        });
    }

    [Fact]
    public void Should_trigger_check_if_handling_all_events()
    {
        TestForTrigger(handleAll: true, schemaId: schemaMatching, condition: null, action: ctx =>
        {
            var @event = new EnrichedContentEvent { SchemaId = schemaMatching };

            var actual = sut.Trigger(@event, ctx.Rule.Trigger);

            Assert.True(actual);
        });
    }

    [Fact]
    public void Should_trigger_check_if_condition_is_empty()
    {
        TestForTrigger(handleAll: false, schemaId: schemaMatching, condition: string.Empty, action: ctx =>
        {
            var @event = new EnrichedContentEvent { SchemaId = schemaMatching };

            var actual = sut.Trigger(@event, ctx.Rule.Trigger);

            Assert.True(actual);
        });
    }

    [Fact]
    public void Should_trigger_check_if_condition_matchs()
    {
        TestForTrigger(handleAll: false, schemaId: schemaMatching, condition: "true", action: ctx =>
        {
            var @event = new EnrichedContentEvent { SchemaId = schemaMatching };

            var actual = sut.Trigger(@event, ctx.Rule.Trigger);

            Assert.True(actual);
        });
    }

    [Fact]
    public void Should_not_trigger_check_if_schema_id_does_not_match()
    {
        TestForTrigger(handleAll: false, schemaId: schemaNotMatching, condition: null, action: ctx =>
        {
            var @event = new EnrichedContentEvent { SchemaId = schemaMatching };

            var actual = sut.Trigger(@event, ctx.Rule.Trigger);

            Assert.False(actual);
        });
    }

    [Fact]
    public void Should_not_trigger_check_if_condition_does_not_match()
    {
        TestForTrigger(handleAll: false, schemaId: schemaMatching, condition: "false", action: ctx =>
        {
            var @event = new EnrichedContentEvent { SchemaId = schemaMatching };

            var actual = sut.Trigger(@event, ctx.Rule.Trigger);

            Assert.False(actual);
        });
    }

    private void TestForTrigger(bool handleAll, NamedId<DomainId>? schemaId, string? condition, Action<RuleContext> action)
    {
        var trigger = new ContentChangedTriggerV2
        {
            HandleAll = handleAll
        };

        if (schemaId != null)
        {
            trigger = trigger with
            {
                Schemas = ReadonlyList.Create(
                    new SchemaCondition
                    {
                        SchemaId = schemaId.Id,
                        Condition = condition
                    })
            };
        }

        action(Context(trigger));

        if (string.IsNullOrWhiteSpace(condition))
        {
            A.CallTo(() => scriptEngine.Evaluate(A<ScriptVars>._, A<string>._, default))
                .MustNotHaveHappened();
        }
        else
        {
            A.CallTo(() => scriptEngine.Evaluate(A<ScriptVars>._, condition, default))
                .MustHaveHappened();
        }
    }

    private void SetupData(ContentEvent @event, int version)
    {
        var dataNow = new ContentData();
        var dataOld = new ContentData();

        A.CallTo(() => contentLoader.GetAsync(AppId.Id, @event.ContentId, version, CancellationToken))
            .Returns(new Content { AppId = AppId, SchemaId = schemaMatching, Version = version, Data = dataNow, Id = @event.ContentId });

        A.CallTo(() => contentLoader.GetAsync(AppId.Id, @event.ContentId, version, CancellationToken))
            .Returns(new Content { AppId = AppId, SchemaId = schemaMatching, Version = version - 1, Data = dataOld });
    }

    private RulesContext ReferencingContext(int? maxEvents, bool allowExtra, NamedId<DomainId>? schemaId = null)
    {
        schemaId ??= schemaMatching;

        var trigger = new ContentChangedTriggerV2
        {
            ReferencedSchemas = new List<SchemaCondition>
            {
                new SchemaCondition
                {
                    SchemaId = schemaId.Id,
                }
            }.ToReadonlyList()
        };

        return new RulesContext
        {
            AppId = AppId,
            MaxEvents = maxEvents,
            IncludeSkipped = true,
            IncludeStale = true,
            Rules = new Dictionary<DomainId, Rule>
            {
                [DomainId.NewGuid()] = CreateRule() with { Trigger = trigger }
            }.ToReadonlyDictionary(),
            AllowExtraEvents = allowExtra
        };
    }

    private RuleContext Context(RuleTrigger? trigger = null)
    {
        trigger ??= new ContentChangedTriggerV2();

        return new RuleContext
        {
            AppId = AppId,
            IncludeSkipped = false,
            IncludeStale = false,
            Rule = CreateRule() with { Trigger = trigger }
        };
    }
}
