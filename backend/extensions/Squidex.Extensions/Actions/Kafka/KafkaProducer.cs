﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using Avro;
using Avro.Generic;
using Confluent.Kafka;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using Microsoft.Extensions.Options;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Log;

namespace Squidex.Extensions.Actions.Kafka
{
    public sealed class KafkaProducer
    {
        private readonly IProducer<string, string> textProducer;
        private readonly IProducer<string, GenericRecord> avroProducer;
        private readonly ISchemaRegistryClient schemaRegistry;
        private readonly IJsonSerializer jsonSerializer;

        public KafkaProducer(IOptions<KafkaProducerOptions> options, ISemanticLog log, IJsonSerializer jsonSerializer)
        {
            this.jsonSerializer = jsonSerializer;

            textProducer = new ProducerBuilder<string, string>(options.Value)
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

            if (options.Value.IsSchemaRegistryConfigured())
            {
                schemaRegistry = new CachedSchemaRegistryClient(options.Value.SchemaRegistry);

                avroProducer = new ProducerBuilder<string, GenericRecord>(options.Value)
                   .SetErrorHandler((p, error) =>
                   {
                       LogError(log, error);
                   })
                   .SetLogHandler((p, message) =>
                   {
                       LogMessage(log, message);
                   })
                   .SetKeySerializer(Serializers.Utf8)
                   .SetValueSerializer(new AvroSerializer<GenericRecord>(schemaRegistry, options.Value.AvroSerializer))
                   .Build();
            }
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

        public async Task<DeliveryResult<string, string>> Send(string topicName, Message<string, string> message, string schema)
        {
            if (!string.IsNullOrWhiteSpace(schema))
            {
                var value = CreateAvroRecord(message.Value, schema);

                var avroMessage = new Message<string, GenericRecord> { Key = message.Key, Headers = message.Headers, Value = value };

                await avroProducer.ProduceAsync(topicName, avroMessage);
            }

            return await textProducer.ProduceAsync(topicName, message);
        }

        private GenericRecord CreateAvroRecord(string json, string avroSchema)
        {
            var schema = (RecordSchema)Avro.Schema.Parse(avroSchema);

            var jsonObject = jsonSerializer.Deserialize<JsonObject>(json);

            var result = (GenericRecord)GetValue(jsonObject, schema);

            return result;
        }

        public void Dispose()
        {
            textProducer?.Dispose();
            avroProducer?.Dispose();
        }

        private object GetValue(IJsonValue value, Avro.Schema schema)
        {
            switch (value)
            {
                case JsonString s:
                    return s.Value;
                case JsonNumber n:
                    return n.Value;
                case JsonBoolean b:
                    return b.Value;
                case JsonObject o:
                    {
                        var recordSchema = (RecordSchema)schema;

                        var result = new GenericRecord(recordSchema);

                        foreach (var (key, childValue) in o)
                        {
                            if (recordSchema.TryGetField(key, out var field))
                            {
                                result.Add(key, GetValue(childValue, field.Schema));
                            }
                        }

                        return result;
                    }

                case JsonArray a:
                    {
                        var arraySchema = (ArraySchema)schema;

                        var result = new List<object>();

                        foreach (var item in a)
                        {
                            result.Add(GetValue(item, arraySchema.ItemSchema));
                        }

                        return result.ToArray();
                    }
            }

            return null;
        }
    }
}
