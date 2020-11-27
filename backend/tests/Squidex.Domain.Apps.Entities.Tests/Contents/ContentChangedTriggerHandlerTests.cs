// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Caching;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public class ContentChangedTriggerHandlerTests
    {
        private readonly IScriptEngine scriptEngine = A.Fake<IScriptEngine>();
        private readonly ILocalCache localCache = new AsyncLocalCache();
        private readonly IContentLoader contentLoader = A.Fake<IContentLoader>();
        private readonly IContentRepository contentRepository = A.Fake<IContentRepository>();
        private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
        private readonly NamedId<DomainId> schemaMatch = NamedId.Of(DomainId.NewGuid(), "my-schema1");
        private readonly NamedId<DomainId> schemaNonMatch = NamedId.Of(DomainId.NewGuid(), "my-schema2");
        private readonly DomainId ruleId = DomainId.NewGuid();
        private readonly IRuleTriggerHandler sut;

        public ContentChangedTriggerHandlerTests()
        {
            A.CallTo(() => scriptEngine.Evaluate(A<ScriptVars>._, "true", default))
                .Returns(true);

            A.CallTo(() => scriptEngine.Evaluate(A<ScriptVars>._, "false", default))
                .Returns(false);

            sut = new ContentChangedTriggerHandler(scriptEngine, contentLoader, contentRepository);
        }

        public static IEnumerable<object[]> TestEvents()
        {
            yield return new object[] { new ContentCreated(), EnrichedContentEventType.Created };
            yield return new object[] { new ContentUpdated(), EnrichedContentEventType.Updated };
            yield return new object[] { new ContentDeleted(), EnrichedContentEventType.Deleted };
            yield return new object[] { new ContentStatusChanged { Change = StatusChange.Change }, EnrichedContentEventType.StatusChanged };
            yield return new object[] { new ContentStatusChanged { Change = StatusChange.Published }, EnrichedContentEventType.Published };
            yield return new object[] { new ContentStatusChanged { Change = StatusChange.Unpublished }, EnrichedContentEventType.Unpublished };
        }

        [Fact]
        public void Should_return_true_when_asking_for_snapshot_support()
        {
            Assert.True(sut.CanCreateSnapshotEvents);
        }

        [Fact]
        public async Task Should_create_events_from_snapshots()
        {
            var trigger = new ContentChangedTriggerV2();

            A.CallTo(() => contentRepository.StreamAll(appId.Id, null))
                .Returns(new List<ContentEntity>
                {
                    new ContentEntity { SchemaId = schemaMatch },
                    new ContentEntity { SchemaId = schemaMatch }
                }.ToAsyncEnumerable());

            var result = await sut.CreateSnapshotEvents(trigger, appId.Id).ToListAsync();

            var typed = result.OfType<EnrichedContentEvent>().ToList();

            Assert.Equal(2, typed.Count);
            Assert.Equal(2, typed.Count(x => x.Type == EnrichedContentEventType.Created));
        }

        [Fact]
        public async Task Should_create_events_from_snapshots_with_schema_ids()
        {
            var trigger = new ContentChangedTriggerV2
            {
                Schemas = new ReadOnlyCollection<ContentChangedTriggerSchemaV2>(new List<ContentChangedTriggerSchemaV2>
                {
                    new ContentChangedTriggerSchemaV2
                    {
                        SchemaId = schemaMatch.Id
                    }
                })
            };

            A.CallTo(() => contentRepository.StreamAll(appId.Id, A<HashSet<DomainId>>.That.Is(schemaMatch.Id)))
                .Returns(new List<ContentEntity>
                {
                    new ContentEntity { SchemaId = schemaMatch },
                    new ContentEntity { SchemaId = schemaMatch }
                }.ToAsyncEnumerable());

            var result = await sut.CreateSnapshotEvents(trigger, appId.Id).ToListAsync();

            var typed = result.OfType<EnrichedContentEvent>().ToList();

            Assert.Equal(2, typed.Count);
            Assert.Equal(2, typed.Count(x => x.Type == EnrichedContentEventType.Created));
        }

        [Theory]
        [MemberData(nameof(TestEvents))]
        public async Task Should_create_enriched_events(ContentEvent @event, EnrichedContentEventType type)
        {
            @event.AppId = appId;

            var envelope = Envelope.Create<AppEvent>(@event).SetEventStreamNumber(12);

            A.CallTo(() => contentLoader.GetAsync(appId.Id, @event.ContentId, 12))
                .Returns(new ContentEntity { AppId = appId, SchemaId = schemaMatch });

            var result = await sut.CreateEnrichedEventsAsync(envelope);

            var enrichedEvent = result.Single() as EnrichedContentEvent;

            Assert.Equal(type, enrichedEvent!.Type);
        }

        [Fact]
        public async Task Should_enrich_with_old_data_when_updated()
        {
            var @event = new ContentUpdated { AppId = appId, ContentId = DomainId.NewGuid() };

            var envelope = Envelope.Create<AppEvent>(@event).SetEventStreamNumber(12);

            var dataNow = new NamedContentData();
            var dataOld = new NamedContentData();

            A.CallTo(() => contentLoader.GetAsync(appId.Id, @event.ContentId, 12))
                .Returns(new ContentEntity { AppId = appId, SchemaId = schemaMatch, Version = 12, Data = dataNow, Id = @event.ContentId });

            A.CallTo(() => contentLoader.GetAsync(appId.Id, @event.ContentId, 11))
                .Returns(new ContentEntity { AppId = appId, SchemaId = schemaMatch, Version = 11, Data = dataOld });

            var result = await sut.CreateEnrichedEventsAsync(envelope);

            var enrichedEvent = result.Single() as EnrichedContentEvent;

            Assert.Same(dataNow, enrichedEvent!.Data);
            Assert.Same(dataOld, enrichedEvent!.DataOld);
        }

        [Fact]
        public void Should_not_trigger_precheck_when_event_type_not_correct()
        {
            TestForTrigger(handleAll: true, schemaId: null, condition: null, action: trigger =>
            {
                var result = sut.Trigger(new AssetCreated(), trigger, ruleId);

                Assert.False(result);
            });
        }

        [Fact]
        public void Should_not_trigger_precheck_when_trigger_contains_no_schemas()
        {
            TestForTrigger(handleAll: false, schemaId: null, condition: null, action: trigger =>
            {
                var result = sut.Trigger(new ContentCreated { SchemaId = schemaMatch }, trigger, ruleId);

                Assert.False(result);
            });
        }

        [Fact]
        public void Should_trigger_precheck_when_handling_all_events()
        {
            TestForTrigger(handleAll: true, schemaId: schemaMatch, condition: null, action: trigger =>
            {
                var result = sut.Trigger(new ContentCreated { SchemaId = schemaMatch }, trigger, ruleId);

                Assert.True(result);
            });
        }

        [Fact]
        public void Should_trigger_precheck_when_condition_is_empty()
        {
            TestForTrigger(handleAll: false, schemaId: schemaMatch, condition: string.Empty, action: trigger =>
            {
                var result = sut.Trigger(new ContentCreated { SchemaId = schemaMatch }, trigger, ruleId);

                Assert.True(result);
            });
        }

        [Fact]
        public void Should_not_trigger_precheck_when_schema_id_does_not_match()
        {
            TestForTrigger(handleAll: false, schemaId: schemaNonMatch, condition: null, action: trigger =>
            {
                var result = sut.Trigger(new ContentCreated { SchemaId = schemaMatch }, trigger, ruleId);

                Assert.False(result);
            });
        }

        [Fact]
        public void Should_not_trigger_check_when_event_type_not_correct()
        {
            TestForTrigger(handleAll: true, schemaId: null, condition: null, action: trigger =>
            {
                var result = sut.Trigger(new EnrichedAssetEvent(), trigger);

                Assert.False(result);
            });
        }

        [Fact]
        public void Should_not_trigger_check_when_trigger_contains_no_schemas()
        {
            TestForTrigger(handleAll: false, schemaId: null, condition: null, action: trigger =>
            {
                var result = sut.Trigger(new EnrichedContentEvent { SchemaId = schemaMatch }, trigger);

                Assert.False(result);
            });
        }

        [Fact]
        public void Should_trigger_check_when_handling_all_events()
        {
            TestForTrigger(handleAll: true, schemaId: schemaMatch, condition: null, action: trigger =>
            {
                var result = sut.Trigger(new EnrichedContentEvent { SchemaId = schemaMatch }, trigger);

                Assert.True(result);
            });
        }

        [Fact]
        public void Should_trigger_check_when_condition_is_empty()
        {
            TestForTrigger(handleAll: false, schemaId: schemaMatch, condition: string.Empty, action: trigger =>
            {
                var result = sut.Trigger(new EnrichedContentEvent { SchemaId = schemaMatch }, trigger);

                Assert.True(result);
            });
        }

        [Fact]
        public void Should_trigger_check_when_condition_matchs()
        {
            TestForTrigger(handleAll: false, schemaId: schemaMatch, condition: "true", action: trigger =>
            {
                var result = sut.Trigger(new EnrichedContentEvent { SchemaId = schemaMatch }, trigger);

                Assert.True(result);
            });
        }

        [Fact]
        public void Should_not_trigger_check_when_schema_id_does_not_match()
        {
            TestForTrigger(handleAll: false, schemaId: schemaNonMatch, condition: null, action: trigger =>
            {
                var result = sut.Trigger(new EnrichedContentEvent { SchemaId = schemaMatch }, trigger);

                Assert.False(result);
            });
        }

        [Fact]
        public void Should_not_trigger_check_when_condition_does_not_matchs()
        {
            TestForTrigger(handleAll: false, schemaId: schemaMatch, condition: "false", action: trigger =>
            {
                var result = sut.Trigger(new EnrichedContentEvent { SchemaId = schemaMatch }, trigger);

                Assert.False(result);
            });
        }

        private void TestForTrigger(bool handleAll, NamedId<DomainId>? schemaId, string? condition, Action<ContentChangedTriggerV2> action)
        {
            var trigger = new ContentChangedTriggerV2 { HandleAll = handleAll };

            if (schemaId != null)
            {
                trigger.Schemas = new ReadOnlyCollection<ContentChangedTriggerSchemaV2>(new List<ContentChangedTriggerSchemaV2>
                {
                    new ContentChangedTriggerSchemaV2
                    {
                        SchemaId = schemaId.Id, Condition = condition
                    }
                });
            }

            action(trigger);

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
    }
}
