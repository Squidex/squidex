// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Infrastructure.Http;
using Squidex.Infrastructure.Plugins;

namespace Squidex.Extensions.Actions.Webhook;

public sealed class WebhookPlugin : IPlugin
{
    public void ConfigureServices(IServiceCollection services, IConfiguration config)
    {
        services.AddHttpClient("FlowClient")
            .EnableSsrfProtection();

        services.AddFlowStep<WebhookFlowStep>();
#pragma warning disable CS0618 // Type or member is obsolete
        services.AddRuleAction<WebhookAction>();
#pragma warning restore CS0618 // Type or member is obsolete
    }
}
