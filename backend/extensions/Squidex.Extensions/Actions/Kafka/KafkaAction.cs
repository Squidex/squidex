// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules.Old;

namespace Squidex.Extensions.Actions.Kafka;

public sealed record KafkaAction : RuleAction<KafkaStep>
{
    public string TopicName { get; set; }

    public string? Payload { get; set; }

    public string? Key { get; set; }

    public string? PartitionKey { get; set; }

    public int PartitionCount { get; set; }

    public string? Headers { get; set; }

    public string? Schema { get; set; }
}
