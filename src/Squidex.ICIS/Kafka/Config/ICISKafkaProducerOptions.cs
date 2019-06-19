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
    public class ICISKafkaProducerOptions : ProducerConfig
    {
        public new string BootstrapServers { get; set; }
    }
}
