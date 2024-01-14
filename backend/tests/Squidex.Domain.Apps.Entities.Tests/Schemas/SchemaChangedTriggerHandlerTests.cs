// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Apps;
using Squidex.Domain.Apps.Events.Schemas;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Entities.Schemas;

public class SchemaChangedTriggerHandlerTests : GivenContext
{
    private readonly IScriptEngine scriptEngine = A.Fake<IScriptEngine>();
    private readonly IRuleTriggerHandler sut;

    public static readonly TheoryData<SchemaEvent, EnrichedSchemaEventType> TestEvents = new ()
    {
        { TestUtils.CreateEvent<SchemaCreated>(), EnrichedSchemaEventType.Created },
        { TestUtils.CreateEvent<SchemaUpdated>(), EnrichedSchemaEventType.Updated },
        { TestUtils.CreateEvent<SchemaDeleted>(), EnrichedSchemaEventType.Deleted },
        { TestUtils.CreateEvent<SchemaPublished>(), EnrichedSchemaEventType.Published },
        { TestUtils.CreateEvent<SchemaUnpublished>(), EnrichedSchemaEventType.Unpublished },
    };

    public SchemaChangedTriggerHandlerTests()
    {
        A.CallTo(() => scriptEngine.Evaluate(A<ScriptVars>._, "true", default))
            .Returns(true);

        A.CallTo(() => scriptEngine.Evaluate(A<ScriptVars>._, "false", default))
            .Returns(false);

        sut = new SchemaChangedTriggerHandler(scriptEngine);
    }

    [Fact]
    public void Should_return_false_if_asking_for_snapshot_support()
    {
        Assert.False(sut.CanCreateSnapshotEvents);
    }

    [Fact]
    public void Should_handle_schema_event()
    {
        Assert.True(sut.Handles(new SchemaCreated()));
    }

    [Fact]
    public void Should_not_handle_other_event()
    {
        Assert.False(sut.Handles(new AppCreated()));
    }

    [Theory]
    [MemberData(nameof(TestEvents))]
    public async Task Should_create_enriched_events(SchemaEvent @event, EnrichedSchemaEventType type)
    {
        var ctx = Context().ToRulesContext();

        var envelope = Envelope.Create<AppEvent>(@event).SetEventStreamNumber(12);

        var actuals = await sut.CreateEnrichedEventsAsync(envelope, ctx, default).ToListAsync();
        var actual = actuals.Single() as EnrichedSchemaEvent;

        Assert.Equal(type, actual!.Type);
        Assert.Equal(@event.Actor, actual.Actor);
        Assert.Equal(@event.AppId, actual.AppId);
        Assert.Equal(@event.AppId.Id, actual.AppId.Id);
        Assert.Equal(@event.SchemaId, actual.SchemaId);
        Assert.Equal(@event.SchemaId.Id, actual.SchemaId.Id);
    }

    [Fact]
    public void Should_trigger_precheck_if_event_type_correct()
    {
        TestForCondition(string.Empty, ctx =>
        {
            var @event = new SchemaCreated();

            var actual = sut.Trigger(Envelope.Create<AppEvent>(@event), ctx.Rule.Trigger);

            Assert.True(actual);
        });
    }

    [Fact]
    public void Should_trigger_check_if_condition_is_empty()
    {
        TestForCondition(string.Empty, ctx =>
        {
            var @event = new EnrichedSchemaEvent();

            var actual = sut.Trigger(@event, ctx.Rule.Trigger);

            Assert.True(actual);
        });
    }

    [Fact]
    public void Should_trigger_check_if_condition_matchs()
    {
        TestForCondition("true", ctx =>
        {
            var @event = new EnrichedSchemaEvent();

            var actual = sut.Trigger(@event, ctx.Rule.Trigger);

            Assert.True(actual);
        });
    }

    [Fact]
    public void Should_not_trigger_check_if_condition_does_not_match()
    {
        TestForCondition("false", ctx =>
        {
            var @event = new EnrichedSchemaEvent();

            var actual = sut.Trigger(@event, ctx.Rule.Trigger);

            Assert.False(actual);
        });
    }

    private void TestForCondition(string condition, Action<RuleContext> action)
    {
        var trigger = new SchemaChangedTrigger
        {
            Condition = condition
        };

        action(Context(trigger));

        if (string.IsNullOrWhiteSpace(condition))
        {
            A.CallTo(() => scriptEngine.Evaluate(A<ScriptVars>._, condition, default))
                .MustNotHaveHappened();
        }
        else
        {
            A.CallTo(() => scriptEngine.Evaluate(A<ScriptVars>._, condition, default))
                .MustHaveHappened();
        }
    }

    private RuleContext Context(RuleTrigger? trigger = null)
    {
        trigger ??= new SchemaChangedTrigger();

        return new RuleContext
        {
            AppId = AppId,
            IncludeSkipped = true,
            IncludeStale = true,
            Rule = CreateRule() with { Trigger = trigger }
        };
    }
}
