// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Entities.Rules.Repositories;
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
    public async Task Should_not_enqueue_result_without_job()
    {
        var result = new JobResult
        {
            SkipReason = SkipReason.None,
        };

        var writes = await EnqueueAndFlushAsync(result);

        Assert.Empty(writes);
    }

    [Theory]
    [InlineData(SkipReason.ConditionDoesNotMatch)]
    [InlineData(SkipReason.ConditionPrecheckDoesNotMatch)]
    [InlineData(SkipReason.FromRule)]
    [InlineData(SkipReason.NoTrigger)]
    [InlineData(SkipReason.TooOld)]
    [InlineData(SkipReason.WrongEvent)]
    [InlineData(SkipReason.WrongEventForTrigger)]
    public async Task Should_not_enqueue_skipped_events_without_error(SkipReason reason)
    {
        var result = new JobResult
        {
            SkipReason = reason,
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
    public async Task Should_enqueue_with_failed_reason()
    {
        var result = new JobResult
        {
            SkipReason = SkipReason.Failed,
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

        var writes = await EnqueueAndFlushAsync(result);

        Assert.Equal(new[]
        {
            new RuleEventWrite(result.Job, null, null),
        }, writes);
    }

    [Fact]
    public async Task Should_enqueue_with_error()
    {
        var result = new JobResult
        {
            SkipReason = SkipReason.Failed,
            EnrichedEvent = new EnrichedManualEvent(),
            EnrichmentError = new InvalidOperationException(),
            Job = new CreateFlowInstanceRequest<FlowEventContext>
            {
                Context = new FlowEventContext(),
                Definition = new FlowDefinition(),
                DefinitionId = Guid.NewGuid().ToString(),
                OwnerId = Guid.NewGuid().ToString(),
            },
        };

        var writes = await EnqueueAndFlushAsync(result);

        Assert.Equal(new[]
        {
            new RuleEventWrite(result.Job, null, result.EnrichmentError),
        }, writes);
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
        };

        var writes = await EnqueueAndFlushAsync(result);

        Assert.Equal(new[]
        {
            new RuleEventWrite(result.Job, result.Job.Created, null),
        }, writes);
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
        };

        var writes = await EnqueueAndFlushAsync(result);

        Assert.Equal(new[]
        {
            new RuleEventWrite(result.Job, result.Job.Created, null),
        }, writes);
    }

    [Fact]
    public async Task Should_write_batched()
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
        };

        for (var i = 0; i < 250; i++)
        {
            await sut.WriteAsync(AppId.Id, result);
        }

        A.CallTo(() => flowManager.EnqueueAsync(A<List<RuleEventWrite>>._, default))
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
