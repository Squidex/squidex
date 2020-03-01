// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using Squidex.Infrastructure.Log;

namespace Squidex.Extensions.Actions.Kafka
{
    public sealed class KafkaProducer
    {
        private readonly IProducer<string, string> producer;

        public KafkaProducer(IOptions<KafkaProducerOptions> options, ISemanticLog log)
        {
            producer = new ProducerBuilder<string, string>(options.Value)
                .SetErrorHandler((p, error) =>
                {
                    LogError(log, error);
                })
                .SetLogHandler((p, message) =>
                {
                    LogMessage(log, message);
                })
                .SetKeySerializer(Serializers.Utf8)
                .SetValueSerializer(Serializers.Utf8)
                .Build();
        }

        private static void LogMessage(ISemanticLog log, LogMessage message)
        {
            var level = SemanticLogLevel.Information;

            switch (message.Level)
            {
                case SyslogLevel.Emergency:
                    level = SemanticLogLevel.Error;
                    break;
                case SyslogLevel.Alert:
                    level = SemanticLogLevel.Error;
                    break;
                case SyslogLevel.Critical:
                    level = SemanticLogLevel.Error;
                    break;
                case SyslogLevel.Error:
                    level = SemanticLogLevel.Error;
                    break;
                case SyslogLevel.Warning:
                    level = SemanticLogLevel.Warning;
                    break;
                case SyslogLevel.Notice:
                    level = SemanticLogLevel.Information;
                    break;
                case SyslogLevel.Info:
                    level = SemanticLogLevel.Information;
                    break;
                case SyslogLevel.Debug:
                    level = SemanticLogLevel.Debug;
                    break;
            }

            log.Log(level, null, w => w
                 .WriteProperty("action", "KafkaAction")
                 .WriteProperty("name", message.Name)
                 .WriteProperty("message", message.Message));
        }

        private static void LogError(ISemanticLog log, Error error)
        {
            log.LogWarning(w => w
                .WriteProperty("action", "KafkaError")
                .WriteProperty("reason", error.Reason));
        }

        public async Task<DeliveryResult<string, string>> Send(string topicName, string key, string value)
        {
            var message = new Message<string, string> { Key = key, Value = value };

            return await producer.ProduceAsync(topicName, message);
        }

        public void Dispose()
        {
            producer?.Dispose();
        }
    }
}
