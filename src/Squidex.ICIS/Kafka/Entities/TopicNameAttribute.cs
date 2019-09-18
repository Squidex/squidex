using System;

namespace Squidex.ICIS.Kafka.Entities
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class TopicNameAttribute : Attribute
    {
        public string Name { get; }

        public TopicNameAttribute(string name)
        {
            Name = name;
        }
    }
}
