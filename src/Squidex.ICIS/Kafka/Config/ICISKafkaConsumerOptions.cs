// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.ICIS.Actions.Kafka
{
    using Confluent.Kafka;

    public class ICISKafkaConsumerOptions : ConsumerConfig
    {
        public new string BootstrapServers { get; set; }
    }
}
