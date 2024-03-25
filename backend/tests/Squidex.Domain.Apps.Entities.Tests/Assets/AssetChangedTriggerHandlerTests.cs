// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.Assets.Repositories;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Entities.Assets;

public class AssetChangedTriggerHandlerTests : GivenContext
{
    private readonly IScriptEngine scriptEngine = A.Fake<IScriptEngine>();
    private readonly IAssetLoader assetLoader = A.Fake<IAssetLoader>();
    private readonly IAssetRepository assetRepository = A.Fake<IAssetRepository>();
    private readonly IRuleTriggerHandler sut;

    public static readonly TheoryData<AssetEvent, EnrichedAssetEventType> TestEvents = new TheoryData<AssetEvent, EnrichedAssetEventType>
    {
        { TestUtils.CreateEvent<AssetCreated>(), EnrichedAssetEventType.Created },
        { TestUtils.CreateEvent<AssetUpdated>(), EnrichedAssetEventType.Updated },
        { TestUtils.CreateEvent<AssetAnnotated>(), EnrichedAssetEventType.Annotated },
        { TestUtils.CreateEvent<AssetDeleted>(), EnrichedAssetEventType.Deleted }
    };

    public AssetChangedTriggerHandlerTests()
    {
        A.CallTo(() => scriptEngine.Evaluate(A<ScriptVars>._, "true", default))
            .Returns(true);

        A.CallTo(() => scriptEngine.Evaluate(A<ScriptVars>._, "false", default))
            .Returns(false);

        sut = new AssetChangedTriggerHandler(scriptEngine, assetLoader, assetRepository);
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

        A.CallTo(() => assetRepository.StreamAll(AppId.Id, CancellationToken))
            .Returns(new List<Asset>
            {
                CreateAsset(),
                CreateAsset()
            }.ToAsyncEnumerable());

        var actual = await sut.CreateSnapshotEventsAsync(ctx, CancellationToken).ToListAsync(CancellationToken);

        var typed = actual.OfType<EnrichedAssetEvent>().ToList();

        Assert.Equal(2, typed.Count);
        Assert.Equal(2, typed.Count(x => x.Type == EnrichedAssetEventType.Created && x.Name == "AssetQueried"));
    }

    [Theory]
    [MemberData(nameof(TestEvents))]
    public async Task Should_create_enriched_events(AssetEvent @event, EnrichedAssetEventType type)
    {
        var ctx = Context().ToRulesContext();

        var envelope = Envelope.Create<AppEvent>(@event).SetEventStreamNumber(12);

        A.CallTo(() => assetLoader.GetAsync(AppId.Id, @event.AssetId, 12, CancellationToken))
            .Returns(CreateAsset());

        var actual = await sut.CreateEnrichedEventsAsync(envelope, ctx, CancellationToken).ToListAsync(CancellationToken);

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

            var actual = sut.Trigger(@event, ctx.Rule.Trigger);

            Assert.True(actual);
        });
    }

    [Fact]
    public void Should_trigger_check_if_condition_matchs()
    {
        TestForCondition("true", ctx =>
        {
            var @event = new EnrichedAssetEvent();

            var actual = sut.Trigger(@event, ctx.Rule.Trigger);

            Assert.True(actual);
        });
    }

    [Fact]
    public void Should_not_trigger_check_if_condition_does_not_matchs()
    {
        TestForCondition("false", ctx =>
        {
            var @event = new EnrichedAssetEvent();

            var actual = sut.Trigger(@event, ctx.Rule.Trigger);

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

    private RuleContext Context(RuleTrigger? trigger = null)
    {
        trigger ??= new AssetChangedTriggerV2();

        return new RuleContext
        {
            AppId = AppId,
            IncludeSkipped = false,
            IncludeStale = false,
            Rule = CreateRule() with { Trigger = trigger }
        };
    }
}
