using System;
using System.Collections.Generic;
using System.Text;
using Avro.Generic;
using Confluent.SchemaRegistry;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Squidex.ICIS.Kafka.Config;
using Squidex.ICIS.Kafka.Consumer;
using Squidex.ICIS.Kafka.Entities;
using Squidex.ICIS.Kafka.Producer;

namespace Squidex.ICIS.Kafka
{
    public static class KafkaServiceExtensions
    {
        public static void AddKafkaServices(IServiceCollection services, IConfiguration config)
        {
            services.Configure<ICISKafkaOptions>(config.GetSection("kafka"));
            AddKafkaRuleExtension(services, config);
            AddKafkaConsumers(services, config);
        }

        private static void AddKafkaRuleExtension(IServiceCollection services, IConfiguration config)
        {
            var kafkaOptions = config.GetSection("kafka").Get<ICISKafkaOptions>();
            if (kafkaOptions.IsProducerConfigured())
            {
                services.TryAddSingleton<ISchemaRegistryClient>(c => new CachedSchemaRegistryClient(kafkaOptions.SchemaRegistry));

                services.AddSingleton<KafkaProducer<Commentary>>();
                services.AddSingleton<KafkaProducer<CommentaryType>>();
                services.AddSingleton<KafkaProducer<Commodity>>();
                services.AddSingleton<KafkaProducer<Region>>();
                services.AddRuleAction<ICISKafkaAction, ICISKafkaActionHandler>();
            }
        }

        private static void AddKafkaConsumers(this IServiceCollection services, IConfiguration config)
        {
            var kafkaOptions = config.GetSection("kafka").Get<ICISKafkaOptions>();
            if (kafkaOptions.IsConsumerConfigured())
            {
                services.TryAddSingleton<ISchemaRegistryClient>(c => new CachedSchemaRegistryClient(kafkaOptions.SchemaRegistry));

                var consumerServices = config.GetSection("kafka:consumers").GetChildren();

                foreach (var service in consumerServices)
                {
                    var option = service.Get<ConsumerOptions>();

                    services.AddSingleton<IHostedService>(c =>
                    {
                        IKafkaConsumer<GenericRecord> consumer = ActivatorUtilities.CreateInstance<KafkaConsumer<GenericRecord>>(c, option);

                        return ActivatorUtilities.CreateInstance<ConsumerService>(c, option, consumer);
                    });
                }
            }
        }
    }
}
