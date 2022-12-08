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
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Apps;
using Squidex.Domain.Apps.Events.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Entities.Schemas;

public class SchemaChangedTriggerHandlerTests
{
    private readonly IScriptEngine scriptEngine = A.Fake<IScriptEngine>();
    private readonly IRuleTriggerHandler sut;

    public SchemaChangedTriggerHandlerTests()
    {
        A.CallTo(() => scriptEngine.Evaluate(A<ScriptVars>._, "true", default))
            .Returns(true);

        A.CallTo(() => scriptEngine.Evaluate(A<ScriptVars>._, "false", default))
            .Returns(false);

        sut = new SchemaChangedTriggerHandler(scriptEngine);
    }

    public static IEnumerable<object[]> TestEvents()
    {
        yield return new object[] { TestUtils.CreateEvent<SchemaCreated>(), EnrichedSchemaEventType.Created };
        yield return new object[] { TestUtils.CreateEvent<SchemaUpdated>(), EnrichedSchemaEventType.Updated };
        yield return new object[] { TestUtils.CreateEvent<SchemaDeleted>(), EnrichedSchemaEventType.Deleted };
        yield return new object[] { TestUtils.CreateEvent<SchemaPublished>(), EnrichedSchemaEventType.Published };
        yield return new object[] { TestUtils.CreateEvent<SchemaUnpublished>(), EnrichedSchemaEventType.Unpublished };
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
        var ctx = Context(appId: @event.AppId);

        var envelope = Envelope.Create<AppEvent>(@event).SetEventStreamNumber(12);

        var actual = await sut.CreateEnrichedEventsAsync(envelope, ctx, default).ToListAsync();

        var enrichedEvent = actual.Single() as EnrichedSchemaEvent;

        Assert.Equal(type, enrichedEvent!.Type);
        Assert.Equal(@event.Actor, enrichedEvent.Actor);
        Assert.Equal(@event.AppId, enrichedEvent.AppId);
        Assert.Equal(@event.AppId.Id, enrichedEvent.AppId.Id);
        Assert.Equal(@event.SchemaId, enrichedEvent.SchemaId);
        Assert.Equal(@event.SchemaId.Id, enrichedEvent.SchemaId.Id);
    }

    [Fact]
    public void Should_trigger_precheck_if_event_type_correct()
    {
        TestForCondition(string.Empty, ctx =>
        {
            var @event = new SchemaCreated();

            var actual = sut.Trigger(Envelope.Create<AppEvent>(@event), ctx);

            Assert.True(actual);
        });
    }

    [Fact]
    public void Should_trigger_check_if_condition_is_empty()
    {
        TestForCondition(string.Empty, ctx =>
        {
            var @event = new EnrichedSchemaEvent();

            var actual = sut.Trigger(@event, ctx);

            Assert.True(actual);
        });
    }

    [Fact]
    public void Should_trigger_check_if_condition_matchs()
    {
        TestForCondition("true", ctx =>
        {
            var @event = new EnrichedSchemaEvent();

            var actual = sut.Trigger(@event, ctx);

            Assert.True(actual);
        });
    }

    [Fact]
    public void Should_not_trigger_check_if_condition_does_not_match()
    {
        TestForCondition("false", ctx =>
        {
            var @event = new EnrichedSchemaEvent();

            var actual = sut.Trigger(@event, ctx);

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

    private static RuleContext Context(RuleTrigger? trigger = null, NamedId<DomainId>? appId = null)
    {
        trigger ??= new SchemaChangedTrigger();

        return new RuleContext
        {
            AppId = appId ?? NamedId.Of(DomainId.NewGuid(), "my-app"),
            Rule = new Rule(trigger, A.Fake<RuleAction>()),
            RuleId = DomainId.NewGuid()
        };
    }
}
