using Squidex.ICIS.Kafka.Consumer;
using System;

namespace Squidex.ICIS.Kafka.Entities
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class TopicNameAttribute : Attribute
    {
        private readonly string name;

        public TopicNameAttribute(string name)
        {
            this.name = name;
        }

        public string GetName(ConsumerOptions options)
        {
            return name.Replace("{environment}", options.Environment);
        }
    }
}
