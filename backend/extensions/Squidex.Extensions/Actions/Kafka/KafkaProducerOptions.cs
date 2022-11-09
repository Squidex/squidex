// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Confluent.Kafka;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;

namespace Squidex.Extensions.Actions.Kafka;

public class KafkaProducerOptions : ProducerConfig
{
    public SchemaRegistryConfig SchemaRegistry { get; set; }

    public AvroSerializerConfig AvroSerializer { get; set; }

    public bool IsProducerConfigured()
    {
        return !string.IsNullOrWhiteSpace(BootstrapServers);
    }

    public bool IsSchemaRegistryConfigured()
    {
        return !string.IsNullOrWhiteSpace(SchemaRegistry?.Url);
    }
}
