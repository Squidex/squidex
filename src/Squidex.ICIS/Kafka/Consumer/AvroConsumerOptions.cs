using System.Collections.Generic;

namespace Squidex.ICIS.Kafka.Consumer
{
    public sealed class AvroConsumerOptions : ConsumerOptions
    {
        public string TopicName { get; set; }

        public string SchemaName { get; set; }

        public Dictionary<string, string> Mapping { get; set; }
    }
}
