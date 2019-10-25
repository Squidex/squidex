﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Squidex.Infrastructure.Plugins;

namespace Squidex.Extensions.Actions.Kafka
{
    public sealed class KafkaPlugin : IPlugin
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration config)
        {
            var kafkaOptions = config.GetSection("kafka").Get<KafkaProducerOptions>();

            if (kafkaOptions.IsProducerConfigured())
            {
                services.AddRuleAction<KafkaAction, KafkaActionHandler>();

                services.AddSingleton<KafkaProducer>();
                services.AddSingleton(Options.Create(kafkaOptions));
            }
        }
    }
}
