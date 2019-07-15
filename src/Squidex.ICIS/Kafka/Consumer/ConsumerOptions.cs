using System.Collections.Generic;

namespace Squidex.ICIS.Kafka.Consumer
{
    public sealed class ConsumerOptions
    {
        public string TopicName { get; set; }

        public string GroupId { get; set; }

        public string AppName { get; set; }

        public string SchemaName { get; set; }

        public string ClientName { get; set; }

        public Dictionary<string, string> Mapping { get; set; }
    }
}
