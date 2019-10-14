#define FAKED_

using Avro.Specific;
using Confluent.SchemaRegistry;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Squidex.ICIS.Kafka.Config;
using Squidex.ICIS.Kafka.Consumer;
using Squidex.ICIS.Kafka.Entities;
using Squidex.ICIS.Kafka.Producer;
using System;
using System.Linq;
using System.Reflection;

namespace Squidex.ICIS.Kafka
{
    public static class KafkaServiceExtensions
    {
        public static void AddKafkaServices(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<ICISKafkaOptions>(
                config.GetSection("kafka"));

            AddKafkaRuleExtension(services, config);
            AddKafkaConsumers(services, config);
        }

        private static void AddKafkaRuleExtension(IServiceCollection services, IConfiguration config)
        {
            var kafkaOptions = config.GetSection("kafka").Get<ICISKafkaOptions>();

            if (kafkaOptions.IsProducerConfiguredForAvro())
            {
                services.TryAddSingleton<ISchemaRegistryClient>(c => new CachedSchemaRegistryClient(kafkaOptions.SchemaRegistry));

                services.RegisterKafkaProducer<Commentary>();
                services.RegisterKafkaProducer<CommentaryType>();

                services.AddRuleAction<ICISKafkaAction, ICISKafkaActionHandler>();
            }
        }

        private static void AddKafkaConsumers(this IServiceCollection services, IConfiguration config)
        {
            var kafkaOptions = config.GetSection("kafka").Get<ICISKafkaOptions>();

            if (kafkaOptions.IsConsumerConfiguredForJson())
            {
                services.Configure<ConsumerOptions>(
                    config.GetSection("kafka:jsonConsumers"));

                services.AddSingleton<JsonKafkaHandler>();

                var types =
                    typeof(KafkaServiceExtensions).Assembly.GetTypes()
                        .Where(x => x.GetCustomAttribute<TopicNameAttribute>() != null);

                foreach (var type in types)
                {
                    ConfigureConsumer(services, type);
                }
            }
        }

        private static void ConfigureConsumer(this IServiceCollection services, Type type)
        {
            services.AddSingleton<IKafkaConsumerService>(c =>
            {
#if FAKED
                var consumer = new FakeConsumer(2000, type);
#else
                var consumer = ActivatorUtilities.CreateInstance<KafkaJsonConsumer>(c, type);
#endif
                var consumerHandler = (IKafkaHandler<IRefDataEntity>)c.GetService<JsonKafkaHandler>();
                var consumerService = ActivatorUtilities.CreateInstance<ConsumerService<IRefDataEntity>>(c, consumer, consumerHandler);

                return consumerService;
            });
        }

        private static void RegisterKafkaProducer<T>(this IServiceCollection services) where T : ISpecificRecord
        {
            services.AddSingleton<IKafkaProducer<T>, KafkaAvroProducer<T>>();
        }
    }
}
