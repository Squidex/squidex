// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.ICIS.Actions.Kafka
{
    public class ICISKafkaOptions
    {
        public ICISKafkaProducerOptions Producer { get; set; }
        public ICISKafkaConsumerOptions Consumer { get; set; }
        public ICISKafkaSchemaRegistryOptions SchemaRegistry { get; set; }

        public bool IsProducerConfigured()
        {
            return this.Producer != null && SchemaRegistry != null && !string.IsNullOrWhiteSpace(Producer.BootstrapServers) && !string.IsNullOrWhiteSpace(SchemaRegistry.SchemaRegistryUrl);
        }

        public bool IsConsumerConfigured()
        {
            return this.Consumer != null && SchemaRegistry != null && !string.IsNullOrWhiteSpace(Consumer.BootstrapServers) && !string.IsNullOrWhiteSpace(SchemaRegistry.SchemaRegistryUrl);
        }
    }
}
