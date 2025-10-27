// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Flows;
using Squidex.Flows.Internal;
using Squidex.Flows.Internal.Execution;

namespace Squidex.Domain.Apps.Entities.Rules;

public class RuleQueueWriterTests : GivenContext
{
    private readonly IFlowManager<FlowEventContext> flowManager = A.Fake<IFlowManager<FlowEventContext>>();
    private readonly IRuleUsageTracker ruleUsageTracker = A.Fake<IRuleUsageTracker>();
    private readonly RuleQueueWriter sut;

    public RuleQueueWriterTests()
    {
        sut = new RuleQueueWriter(flowManager, ruleUsageTracker, null);
    }

    [Fact]
    public async Task Should_not_enqueue_result_without_rule()
    {
        var result = new JobResult
        {
            SkipReason = SkipReason.None,
            EnrichedEvent = new EnrichedManualEvent(),
            EnrichmentError = null,
            Job = new CreateFlowInstanceRequest<FlowEventContext>
            {
                Context = new FlowEventContext(),
                Definition = new FlowDefinition(),
                DefinitionId = Guid.NewGuid().ToString(),
                OwnerId = Guid.NewGuid().ToString(),
            },
        };

        await sut.WriteAsync(AppId.Id, result);
        await sut.FlushAsync();

        A.CallTo(() => flowManager.EnqueueAsync(A<CreateFlowInstanceRequest<FlowEventContext>[]>._, default))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_not_enqueue_result_without_job()
    {
        var result = new JobResult
        {
            SkipReason = SkipReason.None,
            EnrichedEvent = new EnrichedManualEvent(),
            EnrichmentError = null,
            Rule = CreateRule(),
        };

        await sut.WriteAsync(AppId.Id, result);
        await sut.FlushAsync();

        A.CallTo(() => flowManager.EnqueueAsync(A<CreateFlowInstanceRequest<FlowEventContext>[]>._, default))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_not_enqueue_result_with_skip_reason()
    {
        var result = new JobResult
        {
            SkipReason = SkipReason.FromRule,
            EnrichedEvent = new EnrichedManualEvent(),
            EnrichmentError = null,
            Job = new CreateFlowInstanceRequest<FlowEventContext>
            {
                Context = new FlowEventContext(),
                Definition = new FlowDefinition(),
                DefinitionId = Guid.NewGuid().ToString(),
                OwnerId = Guid.NewGuid().ToString(),
            },
            Rule = CreateRule(),
        };

        await sut.WriteAsync(AppId.Id, result);
        await sut.FlushAsync();

        A.CallTo(() => flowManager.EnqueueAsync(A<CreateFlowInstanceRequest<FlowEventContext>[]>._, default))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_enqueue_success()
    {
        var result = new JobResult
        {
            SkipReason = SkipReason.None,
            EnrichedEvent = new EnrichedManualEvent(),
            EnrichmentError = null,
            Job = new CreateFlowInstanceRequest<FlowEventContext>
            {
                Context = new FlowEventContext(),
                Definition = new FlowDefinition(),
                DefinitionId = Guid.NewGuid().ToString(),
                OwnerId = Guid.NewGuid().ToString(),
            },
            Rule = CreateRule(),
        };

        var writes = await EnqueueAndFlushAsync(result);

        Assert.Equal(new[] { result.Job.Value }, writes);
    }

    [Fact]
    public async Task Should_enqueue_disabled()
    {
        var result = new JobResult
        {
            SkipReason = SkipReason.Disabled,
            EnrichedEvent = new EnrichedManualEvent(),
            EnrichmentError = null,
            Job = new CreateFlowInstanceRequest<FlowEventContext>
            {
                Context = new FlowEventContext(),
                Definition = new FlowDefinition(),
                DefinitionId = Guid.NewGuid().ToString(),
                OwnerId = Guid.NewGuid().ToString(),
            },
            Rule = CreateRule(),
        };

        var writes = await EnqueueAndFlushAsync(result);

        Assert.Equal(new[] { result.Job.Value }, writes);
    }

    [Fact]
    public async Task Should_write_batched()
    {
        var result = new JobResult
        {
            SkipReason = default,
            EnrichedEvent = new EnrichedManualEvent(),
            EnrichmentError = null,
            Job = new CreateFlowInstanceRequest<FlowEventContext>
            {
                Context = new FlowEventContext(),
                Definition = new FlowDefinition(),
                DefinitionId = Guid.NewGuid().ToString(),
                OwnerId = Guid.NewGuid().ToString(),
            },
            Rule = CreateRule(),
        };

        for (var i = 0; i < 250; i++)
        {
            await sut.WriteAsync(AppId.Id, result);
        }

        A.CallTo(() => flowManager.EnqueueAsync(A<CreateFlowInstanceRequest<FlowEventContext>[]>._, default))
            .MustHaveHappenedANumberOfTimesMatching(x => x == 2);
    }

    private async Task<CreateFlowInstanceRequest<FlowEventContext>[]> EnqueueAndFlushAsync(JobResult result)
    {
        var writes = Array.Empty<CreateFlowInstanceRequest<FlowEventContext>>();

        A.CallTo(() => flowManager.EnqueueAsync(A<CreateFlowInstanceRequest<FlowEventContext>[]>._, default))
            .Invokes(x => { writes = x.GetArgument<CreateFlowInstanceRequest<FlowEventContext>[]>(0)!; });

        await sut.WriteAsync(AppId.Id, result);
        await sut.FlushAsync();

        return writes;
    }
}
