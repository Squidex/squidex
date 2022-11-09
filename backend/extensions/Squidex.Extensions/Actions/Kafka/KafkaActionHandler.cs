// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;

#pragma warning disable MA0048 // File name must match type name

namespace Squidex.Extensions.Actions.Kafka;

public sealed class KafkaActionHandler : RuleActionHandler<KafkaAction, KafkaJob>
{
    private const string Description = "Push to Kafka";
    private readonly KafkaProducer kafkaProducer;

    public KafkaActionHandler(RuleEventFormatter formatter, KafkaProducer kafkaProducer)
        : base(formatter)
    {
        this.kafkaProducer = kafkaProducer;
    }

    protected override async Task<(string Description, KafkaJob Data)> CreateJobAsync(EnrichedEvent @event, KafkaAction action)
    {
        string value, key;

        if (!string.IsNullOrEmpty(action.Payload))
        {
            value = await FormatAsync(action.Payload, @event);
        }
        else
        {
            value = ToEnvelopeJson(@event);
        }

        if (!string.IsNullOrEmpty(action.Key))
        {
            key = await FormatAsync(action.Key, @event);
        }
        else
        {
            key = @event.Name;
        }

        var ruleJob = new KafkaJob
        {
            TopicName = action.TopicName,
            MessageKey = key,
            MessageValue = value,
            Headers = await ParseHeadersAsync(action.Headers, @event),
            Schema = action.Schema,
            PartitionKey = await FormatAsync(action.PartitionKey, @event),
            PartitionCount = action.PartitionCount
        };

        return (Description, ruleJob);
    }

    private async Task<Dictionary<string, string>> ParseHeadersAsync(string headers, EnrichedEvent @event)
    {
        if (string.IsNullOrWhiteSpace(headers))
        {
            return null;
        }

        var headersDictionary = new Dictionary<string, string>();

        var lines = headers.Split('\n');

        foreach (var line in lines)
        {
            var indexEqual = line.IndexOf('=', StringComparison.Ordinal);

            if (indexEqual > 0 && indexEqual < line.Length - 1)
            {
                var key = line[..indexEqual];
                var val = line[(indexEqual + 1)..];

                val = await FormatAsync(val, @event);

                headersDictionary[key] = val;
            }
        }

        return headersDictionary;
    }

    protected override async Task<Result> ExecuteJobAsync(KafkaJob job,
        CancellationToken ct = default)
    {
        try
        {
            await kafkaProducer.SendAsync(job, ct);

            return Result.Success($"Event pushed to {job.TopicName} kafka topic with {job.MessageKey} message key.");
        }
        catch (Exception ex)
        {
            return Result.Failed(ex, $"Push to Kafka failed: {ex}");
        }
    }
}

public sealed class KafkaJob
{
    public string TopicName { get; set; }

    public string MessageKey { get; set; }

    public string MessageValue { get; set; }

    public string Schema { get; set; }

    public string PartitionKey { get; set; }

    public Dictionary<string, string> Headers { get; set; }

    public int PartitionCount { get; set; }
}
