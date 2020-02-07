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
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Xunit;

#pragma warning disable SA1401 // Fields must be private
#pragma warning disable RECS0070

namespace Squidex.Domain.Apps.Entities.Contents
{
    public class ContentChangedTriggerHandlerTests
    {
        private readonly IScriptEngine scriptEngine = A.Fake<IScriptEngine>();
        private readonly IContentLoader contentLoader = A.Fake<IContentLoader>();
        private readonly IRuleTriggerHandler sut;
        private readonly Guid ruleId = Guid.NewGuid();
        private static readonly NamedId<Guid> SchemaMatch = NamedId.Of(Guid.NewGuid(), "my-schema1");
        private static readonly NamedId<Guid> SchemaNonMatch = NamedId.Of(Guid.NewGuid(), "my-schema2");

        public ContentChangedTriggerHandlerTests()
        {
            A.CallTo(() => scriptEngine.Evaluate("event", A<object>.Ignored, "true"))
                .Returns(true);

            A.CallTo(() => scriptEngine.Evaluate("event", A<object>.Ignored, "false"))
                .Returns(false);

            sut = new ContentChangedTriggerHandler(scriptEngine, contentLoader);
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

        [Theory]
        [MemberData(nameof(TestEvents))]
        public async Task Should_create_enriched_events(ContentEvent @event, EnrichedContentEventType type)
        {
            var envelope = Envelope.Create<AppEvent>(@event).SetEventStreamNumber(12);

            A.CallTo(() => contentLoader.GetAsync(@event.ContentId, 12))
                .Returns(new ContentEntity { SchemaId = SchemaMatch });

            var result = await sut.CreateEnrichedEventsAsync(envelope);

            var enrichedEvent = result.Single() as EnrichedContentEvent;

            Assert.Equal(type, enrichedEvent!.Type);
        }

        [Fact]
        public async Task Should_enrich_with_old_data_when_updated()
        {
            var @event = new ContentUpdated();

            var envelope = Envelope.Create<AppEvent>(@event).SetEventStreamNumber(12);

            var dataNow = new NamedContentData();
            var dataOld = new NamedContentData();

            A.CallTo(() => contentLoader.GetAsync(@event.ContentId, 12))
                .Returns(new ContentEntity { SchemaId = SchemaMatch, Version = 12, Data = dataNow });

            A.CallTo(() => contentLoader.GetAsync(@event.ContentId, 11))
                .Returns(new ContentEntity { SchemaId = SchemaMatch, Version = 11, Data = dataOld });

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
                var result = sut.Trigger(new ContentCreated { SchemaId = SchemaMatch }, trigger, ruleId);

                Assert.False(result);
            });
        }

        [Fact]
        public void Should_trigger_precheck_when_handling_all_events()
        {
            TestForTrigger(handleAll: true, schemaId: SchemaMatch, condition: null, action: trigger =>
            {
                var result = sut.Trigger(new ContentCreated { SchemaId = SchemaMatch }, trigger, ruleId);

                Assert.True(result);
            });
        }

        [Fact]
        public void Should_trigger_precheck_when_condition_is_empty()
        {
            TestForTrigger(handleAll: false, schemaId: SchemaMatch, condition: string.Empty, action: trigger =>
            {
                var result = sut.Trigger(new ContentCreated { SchemaId = SchemaMatch }, trigger, ruleId);

                Assert.True(result);
            });
        }

        [Fact]
        public void Should_not_trigger_precheck_when_schema_id_does_not_match()
        {
            TestForTrigger(handleAll: false, schemaId: SchemaNonMatch, condition: null, action: trigger =>
            {
                var result = sut.Trigger(new ContentCreated { SchemaId = SchemaMatch }, trigger, ruleId);

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
                var result = sut.Trigger(new EnrichedContentEvent { SchemaId = SchemaMatch }, trigger);

                Assert.False(result);
            });
        }

        [Fact]
        public void Should_trigger_check_when_handling_all_events()
        {
            TestForTrigger(handleAll: true, schemaId: SchemaMatch, condition: null, action: trigger =>
            {
                var result = sut.Trigger(new EnrichedContentEvent { SchemaId = SchemaMatch }, trigger);

                Assert.True(result);
            });
        }

        [Fact]
        public void Should_trigger_check_when_condition_is_empty()
        {
            TestForTrigger(handleAll: false, schemaId: SchemaMatch, condition: string.Empty, action: trigger =>
            {
                var result = sut.Trigger(new EnrichedContentEvent { SchemaId = SchemaMatch }, trigger);

                Assert.True(result);
            });
        }

        [Fact]
        public void Should_trigger_check_when_condition_matchs()
        {
            TestForTrigger(handleAll: false, schemaId: SchemaMatch, condition: "true", action: trigger =>
            {
                var result = sut.Trigger(new EnrichedContentEvent { SchemaId = SchemaMatch }, trigger);

                Assert.True(result);
            });
        }

        [Fact]
        public void Should_not_trigger_check_when_schema_id_does_not_match()
        {
            TestForTrigger(handleAll: false, schemaId: SchemaNonMatch, condition: null, action: trigger =>
            {
                var result = sut.Trigger(new EnrichedContentEvent { SchemaId = SchemaMatch }, trigger);

                Assert.False(result);
            });
        }

        [Fact]
        public void Should_not_trigger_check_when_condition_does_not_matchs()
        {
            TestForTrigger(handleAll: false, schemaId: SchemaMatch, condition: "false", action: trigger =>
            {
                var result = sut.Trigger(new EnrichedContentEvent { SchemaId = SchemaMatch }, trigger);

                Assert.False(result);
            });
        }

        private void TestForTrigger(bool handleAll, NamedId<Guid>? schemaId, string? condition, Action<ContentChangedTriggerV2> action)
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
                A.CallTo(() => scriptEngine.Evaluate("event", A<object>.Ignored, A<string>.Ignored))
                    .MustNotHaveHappened();
            }
            else
            {
                A.CallTo(() => scriptEngine.Evaluate("event", A<object>.Ignored, condition))
                    .MustHaveHappened();
            }
        }
    }
}
