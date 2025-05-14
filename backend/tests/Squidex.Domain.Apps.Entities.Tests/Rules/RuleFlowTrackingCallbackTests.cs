// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Flows.Internal.Execution;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Rules;

public class RuleFlowTrackingCallbackTests : GivenContext
{
    private readonly IRuleUsageTracker ruleUsageTracker = A.Fake<IRuleUsageTracker>();
    private readonly RuleFlowTrackingCallback sut;

    public RuleFlowTrackingCallbackTests()
    {
        sut = new RuleFlowTrackingCallback(ruleUsageTracker);
    }

    [Fact]
    public async Task Should_track_usage_with_success()
    {
        var ruleId = DomainId.NewGuid();

        await sut.OnUpdateAsync(
            new FlowExecutionState<FlowEventContext>
            {
                InstanceId = default,
                Context = new FlowEventContext(),
                Definition = null!,
                DefinitionId = ruleId.ToString(),
                OwnerId = AppId.Id.ToString(),
                Status = FlowExecutionStatus.Completed,
            },
            CancellationToken);

        A.CallTo(() => ruleUsageTracker.TrackAsync(
                AppId.Id,
                ruleId,
                A<DateOnly>._,
                0,
                1,
                0,
                CancellationToken))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_track_usage_with_failure()
    {
        var ruleId = DomainId.NewGuid();

        await sut.OnUpdateAsync(
            new FlowExecutionState<FlowEventContext>
            {
                InstanceId = default,
                Context = new FlowEventContext(),
                Definition = null!,
                DefinitionId = ruleId.ToString(),
                OwnerId = AppId.Id.ToString(),
                Status = FlowExecutionStatus.Failed,
            },
            CancellationToken);

        A.CallTo(() => ruleUsageTracker.TrackAsync(
                AppId.Id,
                ruleId,
                A<DateOnly>._,
                0,
                0,
                1,
                CancellationToken))
            .MustHaveHappened();
    }
}
