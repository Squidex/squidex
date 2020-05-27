﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;

namespace Squidex.Extensions.Actions.Kafka
{
    public sealed class KafkaActionHandler : RuleActionHandler<KafkaAction, KafkaJob>
    {
        private const string Description = "Push to Kafka";
        private readonly KafkaProducer kafkaProducer;

        public KafkaActionHandler(RuleEventFormatter formatter, KafkaProducer kafkaProducer)
            : base(formatter)
        {
            this.kafkaProducer = kafkaProducer;
        }

        protected override (string Description, KafkaJob Data) CreateJob(EnrichedEvent @event, KafkaAction action)
        {
            string value, key;

            if (!string.IsNullOrEmpty(action.Payload))
            {
                value = Format(action.Payload, @event);
            }
            else
            {
                value = ToEnvelopeJson(@event);
            }

            if (!string.IsNullOrEmpty(action.Key))
            {
                key = Format(action.Key, @event);
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
                Headers = ParseHeaders(action.Headers, @event)
            };

            return (Description, ruleJob);
        }

        private Dictionary<string, string> ParseHeaders(string headers, EnrichedEvent @event)
        {
            if (string.IsNullOrWhiteSpace(headers))
            {
                return null;
            }

            var headersDictionary = new Dictionary<string, string>();

            var lines = headers.Split('\n');

            foreach (var line in lines)
            {
                var indexEqual = line.IndexOf('=');

                if (indexEqual > 0 && indexEqual < line.Length - 1)
                {
                    var key = line.Substring(0, indexEqual);
                    var val = line.Substring(indexEqual + 1);

                    val = Format(val, @event);

                    headersDictionary[key] = val;
                }
            }

            return headersDictionary;
        }

        protected override async Task<Result> ExecuteJobAsync(KafkaJob job, CancellationToken ct = default)
        {
            try
            {
                var message = new Message<string, string> { Key = job.MessageKey, Value = job.MessageValue };

                if (job.Headers?.Count > 0)
                {
                    message.Headers = new Headers();

                    foreach (var header in job.Headers)
                    {
                        message.Headers.Add(header.Key, Encoding.UTF8.GetBytes(header.Value));
                    }
                }

                await kafkaProducer.Send(job.TopicName, message);

                return Result.Success($"Event pushed to {job.TopicName} kafka topic.");
            }
            catch (Exception ex)
            {
                return Result.Failed(ex, "Push to Kafka failed.");
            }
        }
    }

    public sealed class KafkaJob
    {
        public string TopicName { get; set; }

        public string MessageKey { get; set; }

        public string MessageValue { get; set; }

        public Dictionary<string, string> Headers { get; set; }
    }
}
