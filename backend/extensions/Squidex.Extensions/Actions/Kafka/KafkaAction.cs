// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules.Deprecated;
using Squidex.Flows;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Validation;

namespace Squidex.Extensions.Actions.Kafka;

[Obsolete("Has been replaced by flows.")]
public sealed record KafkaAction : RuleAction
{
    [LocalizedRequired]
    public string TopicName { get; set; }

    public string? Payload { get; set; }

    public string? Key { get; set; }

    public string? Headers { get; set; }

    public string? Schema { get; set; }

    public string? PartitionKey { get; set; }

    public int PartitionCount { get; set; }

    public override FlowStep ToFlowStep()
    {
        return SimpleMapper.Map(this, new KafkaFlowStep());
    }
}
