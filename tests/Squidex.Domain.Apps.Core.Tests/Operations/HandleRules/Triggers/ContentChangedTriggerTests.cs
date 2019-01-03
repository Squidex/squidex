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
using Squidex.Infrastructure;
using Xunit;

#pragma warning disable SA1401 // Fields must be private

namespace Squidex.Domain.Apps.Core.Operations.HandleRules.Triggers
{
    public class ContentChangedTriggerTests
    {
        private readonly IScriptEngine scriptEngine = A.Fake<IScriptEngine>();
        private readonly IRuleTriggerHandler sut;
        private static readonly NamedId<Guid> SchemaMatch = NamedId.Of(Guid.NewGuid(), "my-schema1");
        private static readonly NamedId<Guid> SchemaNonMatch = NamedId.Of(Guid.NewGuid(), "my-schema2");

        public ContentChangedTriggerTests()
        {
            sut = new ContentChangedTriggerHandler(scriptEngine);
        }

        [Fact]
        public void Should_not_trigger_when_o_contains_no_schemas()
        {
            var trigger = new ContentChangedTriggerV2();

            var @event = new EnrichedContentEvent { SchemaId = SchemaMatch };

            var result = sut.Triggers(@event, trigger);

            Assert.False(result);

            A.CallTo(() => scriptEngine.Evaluate(A<string>.Ignored, A<object>.Ignored, A<string>.Ignored))
                .MustNotHaveHappened();
        }

        [Fact]
        public void Should_trigger_when_cathing_all_events()
        {
            var trigger = new ContentChangedTriggerV2 { HandleAll = true };

            var @event = new EnrichedContentEvent { SchemaId = SchemaMatch };

            var result = sut.Triggers(@event, trigger);

            Assert.True(result);

            A.CallTo(() => scriptEngine.Evaluate(A<string>.Ignored, A<object>.Ignored, A<string>.Ignored))
                .MustNotHaveHappened();
        }

        [Fact]
        public void Should_trigger_when_condition_is_null()
        {
            var trigger = new ContentChangedTriggerV2
            {
                Schemas = new ReadOnlyCollection<ContentChangedTriggerSchemaV2>(new List<ContentChangedTriggerSchemaV2>
                {
                    new ContentChangedTriggerSchemaV2
                    {
                        SchemaId = SchemaMatch.Id
                    }
                })
            };

            var @event = new EnrichedContentEvent { SchemaId = SchemaMatch };

            var result = sut.Triggers(@event, trigger);

            Assert.True(result);

            A.CallTo(() => scriptEngine.Evaluate(A<string>.Ignored, A<object>.Ignored, A<string>.Ignored))
                .MustNotHaveHappened();
        }

        [Fact]
        public void Should_not_trigger_when_schema_id_does_not_match()
        {
            var trigger = new ContentChangedTriggerV2
            {
                Schemas = new ReadOnlyCollection<ContentChangedTriggerSchemaV2>(new List<ContentChangedTriggerSchemaV2>
                {
                    new ContentChangedTriggerSchemaV2
                    {
                        SchemaId = SchemaNonMatch.Id
                    }
                })
            };

            var @event = new EnrichedContentEvent { SchemaId = SchemaMatch };

            var result = sut.Triggers(@event, trigger);

            Assert.False(result);

            A.CallTo(() => scriptEngine.Evaluate(A<string>.Ignored, A<object>.Ignored, A<string>.Ignored))
                .MustNotHaveHappened();
        }

        [Fact]
        public void Should_trigger_when_condition_is_empty()
        {
            var trigger = new ContentChangedTriggerV2
            {
                Schemas = new ReadOnlyCollection<ContentChangedTriggerSchemaV2>(new List<ContentChangedTriggerSchemaV2>
                {
                    new ContentChangedTriggerSchemaV2
                    {
                        SchemaId = SchemaMatch.Id, Condition = string.Empty
                    }
                })
            };

            var @event = new EnrichedContentEvent { SchemaId = SchemaMatch };

            var result = sut.Triggers(@event, trigger);

            Assert.True(result);

            A.CallTo(() => scriptEngine.Evaluate(A<string>.Ignored, A<object>.Ignored, A<string>.Ignored))
                .MustNotHaveHappened();
        }

        [Fact]
        public void Should_trigger_when_condition_matchs()
        {
            var trigger = new ContentChangedTriggerV2
            {
                Schemas = new ReadOnlyCollection<ContentChangedTriggerSchemaV2>(new List<ContentChangedTriggerSchemaV2>
                {
                    new ContentChangedTriggerSchemaV2
                    {
                        SchemaId = SchemaMatch.Id, Condition = "true"
                    }
                })
            };

            var @event = new EnrichedContentEvent { SchemaId = SchemaMatch };

            A.CallTo(() => scriptEngine.Evaluate("event", @event, "true"))
                .Returns(true);

            var result = sut.Triggers(@event, trigger);

            Assert.True(result);
        }

        [Fact]
        public void Should_not_trigger_when_condition_does_not_matchs()
        {
            var trigger = new ContentChangedTriggerV2
            {
                Schemas = new ReadOnlyCollection<ContentChangedTriggerSchemaV2>(new List<ContentChangedTriggerSchemaV2>
                {
                    new ContentChangedTriggerSchemaV2
                    {
                        SchemaId = SchemaMatch.Id, Condition = "false"
                    }
                })
            };

            var @event = new EnrichedContentEvent { SchemaId = SchemaMatch };

            A.CallTo(() => scriptEngine.Evaluate("event", @event, "false"))
                .Returns(false);

            var result = sut.Triggers(@event, trigger);

            Assert.False(result);
        }
    }
}
