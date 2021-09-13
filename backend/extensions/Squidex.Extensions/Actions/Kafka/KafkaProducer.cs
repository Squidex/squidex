// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avro;
using Avro.Generic;
using Confluent.Kafka;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using Microsoft.Extensions.Options;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Log;
using Schema = Avro.Schema;

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

        public async Task SendAsync(KafkaJob job,
            CancellationToken ct)
        {
            if (!string.IsNullOrWhiteSpace(job.Schema))
            {
                var value = CreateAvroRecord(job.MessageValue, job.Schema);

                var message = new Message<string, GenericRecord> { Value = value };

                await ProduceAsync(avroProducer, message, job, ct);
            }
            else
            {
                var message = new Message<string, string> { Value = job.MessageValue };

                await ProduceAsync(textProducer, message, job, ct);
            }
        }

        private static async Task ProduceAsync<T>(IProducer<string, T> producer, Message<string, T> message, KafkaJob job,
            CancellationToken ct)
        {
            message.Key = job.MessageKey;

            if (job.Headers?.Count > 0)
            {
                message.Headers = new Headers();

                foreach (var header in job.Headers)
                {
                    message.Headers.Add(header.Key, Encoding.UTF8.GetBytes(header.Value));
                }
            }

            if (!string.IsNullOrWhiteSpace(job.PartitionKey) && job.PartitionCount > 0)
            {
                var partition = Math.Abs(job.PartitionKey.GetHashCode(StringComparison.Ordinal)) % job.PartitionCount;

                await producer.ProduceAsync(new TopicPartition(job.TopicName, partition), message, ct);
            }
            else
            {
                await producer.ProduceAsync(job.TopicName, message, ct);
            }
        }

        private GenericRecord CreateAvroRecord(string json, string avroSchema)
        {
            try
            {
                var schema = (RecordSchema)Schema.Parse(avroSchema);

                var jsonObject = jsonSerializer.Deserialize<JsonObject>(json);

                var result = (GenericRecord)GetValue(jsonObject, schema);

                return result;
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException($"Failed to parse json: {json}, got {ex.Message}", ex);
            }
        }

        public void Dispose()
        {
            textProducer?.Dispose();
            avroProducer?.Dispose();
        }

        private static object GetValue(IJsonValue value, Schema schema)
        {
            switch (value)
            {
                case JsonString s when IsTypeOrUnionWith(schema, Schema.Type.String):
                    return s.Value;
                case JsonNumber n when IsTypeOrUnionWith(schema, Schema.Type.Long):
                    return (long)n.Value;
                case JsonNumber n when IsTypeOrUnionWith(schema, Schema.Type.Float):
                    return (float)n.Value;
                case JsonNumber n when IsTypeOrUnionWith(schema, Schema.Type.Int):
                    return (int)n.Value;
                case JsonNumber n when IsTypeOrUnionWith(schema, Schema.Type.Double):
                    return n.Value;
                case JsonBoolean b when IsTypeOrUnionWith(schema, Schema.Type.Boolean):
                    return b.Value;
                case JsonObject o when IsTypeOrUnionWith(schema, Schema.Type.Map):
                    {
                        var mapResult = new Dictionary<string, object>();

                        if (schema is UnionSchema union)
                        {
                            var map = (MapSchema)union.Schemas.FirstOrDefault(x => x.Tag == Schema.Type.Map);

                            foreach (var (key, childValue) in o)
                            {
                                mapResult.Add(key, GetValue(childValue, map?.ValueSchema));
                            }
                        }
                        else if (schema is MapSchema map)
                        {
                            foreach (var (key, childValue) in o)
                            {
                                mapResult.Add(key, GetValue(childValue, map?.ValueSchema));
                            }
                        }

                        return mapResult;
                    }

                case JsonObject o when IsTypeOrUnionWith(schema, Schema.Type.Record):
                    {
                        GenericRecord result = null;

                        if (schema is UnionSchema union)
                        {
                            var record = (RecordSchema)union.Schemas.FirstOrDefault(x => x.Tag == Schema.Type.Record);

                            result = new GenericRecord(record);

                            foreach (var (key, childValue) in o)
                            {
                                if (record != null && record.TryGetField(key, out var field))
                                {
                                    result.Add(key, GetValue(childValue, field.Schema));
                                }
                            }
                        }
                        else if (schema is RecordSchema record)
                        {
                            result = new GenericRecord(record);

                            foreach (var (key, childValue) in o)
                            {
                                if (record.TryGetField(key, out var field))
                                {
                                    result.Add(key, GetValue(childValue, field.Schema));
                                }
                            }
                        }

                        return result;
                    }

                case JsonArray a when IsTypeOrUnionWith(schema, Schema.Type.Array):
                    {
                        var result = new List<object>();

                        if (schema is UnionSchema union)
                        {
                            var arraySchema = (ArraySchema)union.Schemas.FirstOrDefault(x => x.Tag == Schema.Type.Array);

                            foreach (var item in a)
                            {
                                result.Add(GetValue(item, arraySchema?.ItemSchema));
                            }
                        }
                        else if (schema is ArraySchema array)
                        {
                            foreach (var item in a)
                            {
                                result.Add(GetValue(item, array.ItemSchema));
                            }
                        }

                        return result.ToArray();
                    }
            }

            return null;
        }

        private static bool IsTypeOrUnionWith(Schema schema, Schema.Type expected)
        {
            return schema.Tag == expected || (schema is UnionSchema union && union.Schemas.Any(x => x.Tag == expected));
        }
    }
}
