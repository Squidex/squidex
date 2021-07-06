// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;
using Squidex.Infrastructure.EventSourcing;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public class ContentChangedTriggerHandlerTests
    {
        private readonly IScriptEngine scriptEngine = A.Fake<IScriptEngine>();
        private readonly IContentLoader contentLoader = A.Fake<IContentLoader>();
        private readonly IContentRepository contentRepository = A.Fake<IContentRepository>();
        private readonly NamedId<DomainId> schemaMatch = NamedId.Of(DomainId.NewGuid(), "my-schema1");
        private readonly NamedId<DomainId> schemaNonMatch = NamedId.Of(DomainId.NewGuid(), "my-schema2");
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
            var @event = new ContentCreated { SchemaId = schemaMatch };

            Assert.Equal("MySchema1Created", sut.GetName(@event));
        }

        [Fact]
        public void Should_calculate_name_for_deleted()
        {
            var @event = new ContentDeleted { SchemaId = schemaMatch };

            Assert.Equal("MySchema1Deleted", sut.GetName(@event));
        }

        [Fact]
        public void Should_calculate_name_for_updated()
        {
            var @event = new ContentUpdated { SchemaId = schemaMatch };

            Assert.Equal("MySchema1Updated", sut.GetName(@event));
        }

        [Fact]
        public void Should_calculate_name_for_published()
        {
            var @event = new ContentStatusChanged { SchemaId = schemaMatch, Change = StatusChange.Published };

            Assert.Equal("MySchema1Published", sut.GetName(@event));
        }

        [Fact]
        public void Should_calculate_name_for_unpublished()
        {
            var @event = new ContentStatusChanged { SchemaId = schemaMatch, Change = StatusChange.Unpublished };

            Assert.Equal("MySchema1Unpublished", sut.GetName(@event));
        }

        [Fact]
        public void Should_calculate_name_for_status_change()
        {
            var @event = new ContentStatusChanged { SchemaId = schemaMatch, Change = StatusChange.Change };

            Assert.Equal("MySchema1StatusChanged", sut.GetName(@event));
        }

        [Fact]
        public async Task Should_create_events_from_snapshots()
        {
            var ctx = Context();

            A.CallTo(() => contentRepository.StreamAll(ctx.AppId.Id, null, default))
                .Returns(new List<ContentEntity>
                {
                    new ContentEntity { SchemaId = schemaMatch },
                    new ContentEntity { SchemaId = schemaNonMatch }
                }.ToAsyncEnumerable());

            var result = await sut.CreateSnapshotEventsAsync(ctx, default).ToListAsync();

            var typed = result.OfType<EnrichedContentEvent>().ToList();

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
                Schemas = ImmutableList.Create(
                    new ContentChangedTriggerSchemaV2
                    {
                        SchemaId = schemaMatch.Id
                    })
            };

            var ctx = Context(trigger);

            A.CallTo(() => contentRepository.StreamAll(ctx.AppId.Id, A<HashSet<DomainId>>.That.Is(schemaMatch.Id), default))
                .Returns(new List<ContentEntity>
                {
                    new ContentEntity { SchemaId = schemaMatch },
                    new ContentEntity { SchemaId = schemaMatch }
                }.ToAsyncEnumerable());

            var result = await sut.CreateSnapshotEventsAsync(ctx, default).ToListAsync();

            var typed = result.OfType<EnrichedContentEvent>().ToList();

            Assert.Equal(2, typed.Count);
            Assert.Equal(2, typed.Count(x => x.Type == EnrichedContentEventType.Created));
        }

        [Theory]
        [MemberData(nameof(TestEvents))]
        public async Task Should_create_enriched_events(ContentEvent @event, EnrichedContentEventType type)
        {
            var ctx = Context();

            @event.AppId = ctx.AppId;
            @event.SchemaId = schemaMatch;

            var envelope = Envelope.Create<AppEvent>(@event).SetEventStreamNumber(12);

            A.CallTo(() => contentLoader.GetAsync(ctx.AppId.Id, @event.ContentId, 12))
                .Returns(new ContentEntity { AppId = ctx.AppId, SchemaId = schemaMatch });

            var result = await sut.CreateEnrichedEventsAsync(envelope, ctx, default).ToListAsync();

            var enrichedEvent = result.Single() as EnrichedContentEvent;

            Assert.Equal(type, enrichedEvent!.Type);
        }

        [Fact]
        public async Task Should_enrich_with_old_data_if_updated()
        {
            var ctx = Context();

            var @event = new ContentUpdated { AppId = ctx.AppId, ContentId = DomainId.NewGuid(), SchemaId = schemaMatch };

            var envelope = Envelope.Create<AppEvent>(@event).SetEventStreamNumber(12);

            var dataNow = new ContentData();
            var dataOld = new ContentData();

            A.CallTo(() => contentLoader.GetAsync(ctx.AppId.Id, @event.ContentId, 12))
                .Returns(new ContentEntity { AppId = ctx.AppId, SchemaId = schemaMatch, Version = 12, Data = dataNow, Id = @event.ContentId });

            A.CallTo(() => contentLoader.GetAsync(ctx.AppId.Id, @event.ContentId, 11))
                .Returns(new ContentEntity { AppId = ctx.AppId, SchemaId = schemaMatch, Version = 11, Data = dataOld });

            var result = await sut.CreateEnrichedEventsAsync(envelope, ctx, default).ToListAsync();

            var enrichedEvent = result.Single() as EnrichedContentEvent;

            Assert.Same(dataNow, enrichedEvent!.Data);
            Assert.Same(dataOld, enrichedEvent!.DataOld);
        }

        [Fact]
        public void Should_not_trigger_precheck_if_trigger_contains_no_schemas()
        {
            TestForTrigger(handleAll: false, schemaId: null, condition: null, action: ctx =>
            {
                var @event = new ContentCreated { SchemaId = schemaMatch };

                var result = sut.Trigger(Envelope.Create<AppEvent>(@event), ctx);

                Assert.False(result);
            });
        }

        [Fact]
        public void Should_trigger_precheck_if_handling_all_events()
        {
            TestForTrigger(handleAll: true, schemaId: schemaMatch, condition: null, action: ctx =>
            {
                var @event = new ContentCreated { SchemaId = schemaMatch };

                var result = sut.Trigger(Envelope.Create<AppEvent>(@event), ctx);

                Assert.True(result);
            });
        }

        [Fact]
        public void Should_trigger_precheck_if_condition_is_empty()
        {
            TestForTrigger(handleAll: false, schemaId: schemaMatch, condition: string.Empty, action: ctx =>
            {
                var @event = new ContentCreated { SchemaId = schemaMatch };

                var result = sut.Trigger(Envelope.Create<AppEvent>(@event), ctx);

                Assert.True(result);
            });
        }

        [Fact]
        public void Should_not_trigger_precheck_if_schema_id_does_not_match()
        {
            TestForTrigger(handleAll: false, schemaId: schemaNonMatch, condition: null, action: ctx =>
            {
                var @event = new ContentCreated { SchemaId = schemaMatch };

                var result = sut.Trigger(Envelope.Create<AppEvent>(@event), ctx);

                Assert.False(result);
            });
        }

        [Fact]
        public void Should_not_trigger_check_if_trigger_contains_no_schemas()
        {
            TestForTrigger(handleAll: false, schemaId: null, condition: null, action: ctx =>
            {
                var @event = new EnrichedContentEvent { SchemaId = schemaMatch };

                var result = sut.Trigger(@event, ctx);

                Assert.False(result);
            });
        }

        [Fact]
        public void Should_trigger_check_if_handling_all_events()
        {
            TestForTrigger(handleAll: true, schemaId: schemaMatch, condition: null, action: ctx =>
            {
                var @event = new EnrichedContentEvent { SchemaId = schemaMatch };

                var result = sut.Trigger(@event, ctx);

                Assert.True(result);
            });
        }

        [Fact]
        public void Should_trigger_check_if_condition_is_empty()
        {
            TestForTrigger(handleAll: false, schemaId: schemaMatch, condition: string.Empty, action: ctx =>
            {
                var @event = new EnrichedContentEvent { SchemaId = schemaMatch };

                var result = sut.Trigger(@event, ctx);

                Assert.True(result);
            });
        }

        [Fact]
        public void Should_trigger_check_if_condition_matchs()
        {
            TestForTrigger(handleAll: false, schemaId: schemaMatch, condition: "true", action: ctx =>
            {
                var @event = new EnrichedContentEvent { SchemaId = schemaMatch };

                var result = sut.Trigger(@event, ctx);

                Assert.True(result);
            });
        }

        [Fact]
        public void Should_not_trigger_check_if_schema_id_does_not_match()
        {
            TestForTrigger(handleAll: false, schemaId: schemaNonMatch, condition: null, action: ctx =>
            {
                var @event = new EnrichedContentEvent { SchemaId = schemaMatch };

                var result = sut.Trigger(@event, ctx);

                Assert.False(result);
            });
        }

        [Fact]
        public void Should_not_trigger_check_if_condition_does_not_match()
        {
            TestForTrigger(handleAll: false, schemaId: schemaMatch, condition: "false", action: ctx =>
            {
                var @event = new EnrichedContentEvent { SchemaId = schemaMatch };

                var result = sut.Trigger(@event, ctx);

                Assert.False(result);
            });
        }

        private void TestForTrigger(bool handleAll, NamedId<DomainId>? schemaId, string? condition, Action<RuleContext> action)
        {
            var trigger = new ContentChangedTriggerV2 { HandleAll = handleAll };

            if (schemaId != null)
            {
                trigger = trigger with
                {
                    Schemas = ImmutableList.Create(
                        new ContentChangedTriggerSchemaV2
                        {
                            SchemaId = schemaId.Id, Condition = condition
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

        private static RuleContext Context(RuleTrigger? trigger = null)
        {
            trigger ??= new ContentChangedTriggerV2();

            return new RuleContext
            {
                AppId = NamedId.Of(DomainId.NewGuid(), "my-app"),
                Rule = new Rule(trigger, A.Fake<RuleAction>()),
                RuleId = DomainId.NewGuid()
            };
        }
    }
}
