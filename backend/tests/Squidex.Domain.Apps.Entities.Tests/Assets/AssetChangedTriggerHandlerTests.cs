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
using Squidex.Domain.Apps.Entities.Assets.Repositories;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Entities.Assets;

public class AssetChangedTriggerHandlerTests
{
    private readonly CancellationTokenSource cts = new CancellationTokenSource();
    private readonly CancellationToken ct;
    private readonly IScriptEngine scriptEngine = A.Fake<IScriptEngine>();
    private readonly IAssetLoader assetLoader = A.Fake<IAssetLoader>();
    private readonly IAssetRepository assetRepository = A.Fake<IAssetRepository>();
    private readonly IRuleTriggerHandler sut;

    public AssetChangedTriggerHandlerTests()
    {
        ct = cts.Token;

        A.CallTo(() => scriptEngine.Evaluate(A<ScriptVars>._, "true", default))
            .Returns(true);

        A.CallTo(() => scriptEngine.Evaluate(A<ScriptVars>._, "false", default))
            .Returns(false);

        sut = new AssetChangedTriggerHandler(scriptEngine, assetLoader, assetRepository);
    }

    public static IEnumerable<object[]> TestEvents()
    {
        yield return new object[] { TestUtils.CreateEvent<AssetCreated>(), EnrichedAssetEventType.Created };
        yield return new object[] { TestUtils.CreateEvent<AssetUpdated>(), EnrichedAssetEventType.Updated };
        yield return new object[] { TestUtils.CreateEvent<AssetAnnotated>(), EnrichedAssetEventType.Annotated };
        yield return new object[] { TestUtils.CreateEvent<AssetDeleted>(), EnrichedAssetEventType.Deleted };
    }

    [Fact]
    public void Should_return_true_if_asking_for_snapshot_support()
    {
        Assert.True(sut.CanCreateSnapshotEvents);
    }

    [Fact]
    public void Should_handle_asset_event()
    {
        Assert.True(sut.Handles(new AssetCreated()));
    }

    [Fact]
    public void Should_not_handle_asset_moved_event()
    {
        Assert.False(sut.Handles(new AssetMoved()));
    }

    [Fact]
    public void Should_not_handle_other_event()
    {
        Assert.False(sut.Handles(new ContentCreated()));
    }

    [Fact]
    public async Task Should_create_events_from_snapshots()
    {
        var ctx = Context();

        A.CallTo(() => assetRepository.StreamAll(ctx.AppId.Id, ct))
            .Returns(new List<AssetEntity>
            {
                new AssetEntity(),
                new AssetEntity()
            }.ToAsyncEnumerable());

        var actual = await sut.CreateSnapshotEventsAsync(ctx, ct).ToListAsync(ct);

        var typed = actual.OfType<EnrichedAssetEvent>().ToList();

        Assert.Equal(2, typed.Count);
        Assert.Equal(2, typed.Count(x => x.Type == EnrichedAssetEventType.Created && x.Name == "AssetQueried"));
    }

    [Theory]
    [MemberData(nameof(TestEvents))]
    public async Task Should_create_enriched_events(AssetEvent @event, EnrichedAssetEventType type)
    {
        var ctx = Context(appId: @event.AppId);

        var envelope = Envelope.Create<AppEvent>(@event).SetEventStreamNumber(12);

        A.CallTo(() => assetLoader.GetAsync(ctx.AppId.Id, @event.AssetId, 12, ct))
            .Returns(new AssetEntity());

        var actual = await sut.CreateEnrichedEventsAsync(envelope, ctx, ct).ToListAsync(ct);

        var enrichedEvent = (EnrichedAssetEvent)actual.Single();

        Assert.Equal(type, enrichedEvent.Type);
        Assert.Equal(@event.Actor, enrichedEvent.Actor);
        Assert.Equal(@event.AppId, enrichedEvent.AppId);
        Assert.Equal(@event.AppId.Id, enrichedEvent.AppId.Id);
    }

    [Fact]
    public void Should_trigger_check_if_condition_is_empty()
    {
        TestForCondition(string.Empty, ctx =>
        {
            var @event = new EnrichedAssetEvent();

            var actual = sut.Trigger(@event, ctx);

            Assert.True(actual);
        });
    }

    [Fact]
    public void Should_trigger_check_if_condition_matchs()
    {
        TestForCondition("true", ctx =>
        {
            var @event = new EnrichedAssetEvent();

            var actual = sut.Trigger(@event, ctx);

            Assert.True(actual);
        });
    }

    [Fact]
    public void Should_not_trigger_check_if_condition_does_not_matchs()
    {
        TestForCondition("false", ctx =>
        {
            var @event = new EnrichedAssetEvent();

            var actual = sut.Trigger(@event, ctx);

            Assert.False(actual);
        });
    }

    private void TestForCondition(string condition, Action<RuleContext> action)
    {
        var trigger = new AssetChangedTriggerV2
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
        trigger ??= new AssetChangedTriggerV2();

        return new RuleContext
        {
            AppId = appId ?? NamedId.Of(DomainId.NewGuid(), "my-app"),
            Rule = new Rule(trigger, A.Fake<RuleAction>()),
            RuleId = DomainId.NewGuid()
        };
    }
}
