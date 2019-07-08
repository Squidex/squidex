using System;

namespace Squidex.ICIS.Kafka.Entities
{
    public sealed class TopicNameAttribute : Attribute
    {
        public string Name { get; }

        public TopicNameAttribute(string name)
        {
            Name = name;
        }
    }
}
