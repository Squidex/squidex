// ==========================================================================
//  ICIS Copyright
// ==========================================================================
//  Copyright (c) ICIS
//  All rights reserved.
// ==========================================================================

using Avro.Generic;
using Confluent.SchemaRegistry;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Squidex.ICIS.Handlers;
using Squidex.ICIS.Interfaces;
using Squidex.ICIS.Kafka;
using Squidex.ICIS.Kafka.Config;
using Squidex.ICIS.Kafka.Consumer;
using Squidex.ICIS.Kafka.Entities;
using Squidex.ICIS.Kafka.Producer;

namespace Squidex.ICIS.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static void AddIcisServices(this IServiceCollection services, IConfiguration config)
        {
            // Create MyIdentityOptionExtension and call GenesisAuth from here
            services.Configure<ICISKafkaOptions>(config.GetSection("kafka"));
            services.AddKafkaRuleExtention(config);
            services.AddKafkaConsumers(config);
        }

        public static void AddGenesisAuthentication(this IServiceCollection services, string authServer)
        {
            services.AddSingleton<IUserManager, UserManager>();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Audience = $"{authServer}/resources";
                    options.Authority = authServer;
                    options.RequireHttpsMetadata = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidIssuer = authServer
                    };
                    options.Events = new AuthEventsHandler();
                })
                .AddCookie();
        }

        public static void AddKafkaRuleExtention(this IServiceCollection services, IConfiguration config)
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

        public static void AddKafkaConsumers(this IServiceCollection services, IConfiguration config)
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