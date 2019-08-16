using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Extensions.Actions.Kafka;
using Squidex.Extensions.SelfHosted.Kafka;
using System;
using System.Collections.Generic;
using System.Text;

namespace Squidex.Extensions.SelfHosted
{
    public static class KafkaServiceExtensions
    {
        public static void AddKafkaServices(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<KafkaProducerOptions>(config.GetSection("kafka"));
            AddKafkaRuleExtension(services, config);
        }

        private static void AddKafkaRuleExtension(IServiceCollection services, IConfiguration config)
        {
            var kafkaOptions = config.GetSection("kafka").Get<KafkaProducerOptions>();
            if (kafkaOptions.IsProducerConfigured())
            {
                services.AddSingleton<KafkaProducer>();
                services.AddRuleAction<KafkaAction, KafkaActionHandler>();
            }
        }
    }
}
