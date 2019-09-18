// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.ICIS.Kafka.Config
{
    public class ICISKafkaOptions
    {
        public ICISKafkaProducerOptions Producer { get; set; }
        public ICISKafkaConsumerOptions Consumer { get; set; }
        public ICISKafkaSchemaRegistryOptions SchemaRegistry { get; set; }

        public bool IsProducerConfiguredForAvro()
        {
            return !string.IsNullOrWhiteSpace(Producer?.BootstrapServers) && !string.IsNullOrWhiteSpace(SchemaRegistry?.SchemaRegistryUrl);
        }

        public bool IsConsumerConfiguredForAvro()
        {
            return !string.IsNullOrWhiteSpace(Consumer?.BootstrapServers) && !string.IsNullOrWhiteSpace(SchemaRegistry?.SchemaRegistryUrl);
        }

        public bool IsProducerConfiguredForJson()
        {
            return !string.IsNullOrWhiteSpace(Producer?.BootstrapServers);
        }

        public bool IsConsumerConfiguredForJson()
        {
            return !string.IsNullOrWhiteSpace(Consumer?.BootstrapServers);
        }
    }
}
