using System.Collections.Generic;

namespace Squidex.ICIS.Kafka.Consumer
{
    public class ConsumerOptions
    {
        public string GroupId { get; set; }

        public string AppName { get; set; }

        public string ClientName { get; set; }

        public string Environment { get; set; } = "integ";

        public int Version { get; set; } = 1;

        public Dictionary<string, string> TopicNames { get; set; } = new Dictionary<string, string>();
    }
}
