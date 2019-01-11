// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using FakeItEasy;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.HandleRules.EnrichedEvents;
using Squidex.Domain.Apps.Core.HandleRules.Triggers;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Infrastructure;
using Xunit;

#pragma warning disable SA1401 // Fields must be private
#pragma warning disable RECS0070

namespace Squidex.Domain.Apps.Core.Operations.HandleRules.Triggers
{
    public class ContentChangedTriggerTests
    {
        private readonly IScriptEngine scriptEngine = A.Fake<IScriptEngine>();
        private readonly IRuleTriggerHandler sut;
        private readonly Guid ruleId = Guid.NewGuid();
        private static readonly NamedId<Guid> SchemaMatch = NamedId.Of(Guid.NewGuid(), "my-schema1");
        private static readonly NamedId<Guid> SchemaNonMatch = NamedId.Of(Guid.NewGuid(), "my-schema2");

        public ContentChangedTriggerTests()
        {
            sut = new ContentChangedTriggerHandler(scriptEngine);

            A.CallTo(() => scriptEngine.Evaluate("event", A<object>.Ignored, "true"))
                .Returns(true);

            A.CallTo(() => scriptEngine.Evaluate("event", A<object>.Ignored, "false"))
                .Returns(false);
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

        private void TestForTrigger(bool handleAll, NamedId<Guid> schemaId, string condition, Action<ContentChangedTriggerV2> action)
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
                A.CallTo(() => scriptEngine.Evaluate("event", A<object>.Ignored, condition))
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
