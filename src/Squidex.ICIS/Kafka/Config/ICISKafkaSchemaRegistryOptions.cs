// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Confluent.Kafka;
using Confluent.SchemaRegistry;

namespace Squidex.ICIS.Actions.Kafka
{
    public class ICISKafkaSchemaRegistryOptions : SchemaRegistryConfig
    {
        public new string SchemaRegistryUrl { get; set; }
        public new int SchemaRegistryRequestTimeoutMs { get; set; }
        public new int SchemaRegistryMaxCachedSchemas { get; set; }
    }
}
