// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

#if INCLUDE_KAFKA
namespace Squidex.Extensions.Actions.Kafka;

public sealed class KafkaMessageRequest
{
    public string TopicName { get; set; }

    public string? MessageKey { get; set; }

    public string? MessageValue { get; set; }

    public string? Schema { get; set; }

    public string? PartitionKey { get; set; }

    public Dictionary<string, string>? Headers { get; set; }

    public int PartitionCount { get; set; }
}
#endif
