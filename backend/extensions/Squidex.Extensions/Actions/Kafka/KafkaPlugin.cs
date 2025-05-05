// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

#if INCLUDE_KAFKA
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Squidex.Infrastructure.Plugins;

namespace Squidex.Extensions.Actions.Kafka;

public sealed class KafkaPlugin : IPlugin
{
    public void ConfigureServices(IServiceCollection services, IConfiguration config)
    {
        var options = config.GetSection("kafka").Get<KafkaProducerOptions>() ?? new ();

        if (options.IsProducerConfigured())
        {
            services.AddFlowStep<KafkaFlowStep>();
#pragma warning disable CS0618 // Type or member is obsolete
            services.AddRuleAction<KafkaAction>();
#pragma warning restore CS0618 // Type or member is obsolete

            services.AddSingleton<KafkaProducer>();
            services.AddSingleton(Options.Create(options));
        }
    }
}
#endif
