// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Confluent.Kafka;

namespace Squidex.Extensions.Actions.Kafka
{
    public class KafkaProducerOptions : ProducerConfig
    {
        public bool IsProducerConfigured()
        {
            return !string.IsNullOrWhiteSpace(BootstrapServers);
        }
    }
}
