// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Text;

namespace Squidex.ICIS.Actions.Kafka
{
    public class ICISKafkaOptions
    {
        public ICISKafkaProducerOptions Producer { get; set; }
        public ICISKafkaSchemaRegistryOptions SchemaRegistry { get; set; }

        public bool IsConfigured()
        {
            return this.Producer != null && SchemaRegistry != null && !string.IsNullOrWhiteSpace(Producer.BootstrapServers) && !string.IsNullOrWhiteSpace(SchemaRegistry.SchemaRegistryUrl);
        }
    }
}
