using Squidex.ICIS.Kafka.Consumer;
using System;

namespace Squidex.ICIS.Kafka.Entities
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class TopicNameAttribute : Attribute
    {
        private readonly string name;

        public string ConfigurationSource { get; set; }

        public TopicNameAttribute(string name)
        {
            this.name = name;
        }

        public string GetName(ConsumerOptions options)
        {
            if (!string.IsNullOrWhiteSpace(ConfigurationSource) &&
                options.TopicNames != null &&
                options.TopicNames.TryGetValue(ConfigurationSource, out var topicName) &&
                !string.IsNullOrWhiteSpace(topicName))
            {
                return topicName;
            }

            topicName =
                name.Replace("{environment}", options.Environment)
                    .Replace("{version}", options.Version.ToString());

            return topicName;
        }
    }
}
