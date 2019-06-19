// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.ICIS.Actions.Kafka
{
    public sealed class ICISKafkaOptions
    {
        public string Broker { get; set; }

        public string SchemaRegistry { get; set; }

        public bool IsConfigured()
        {
            return !string.IsNullOrWhiteSpace(Broker) && !string.IsNullOrWhiteSpace(SchemaRegistry);
        }
    }
}
