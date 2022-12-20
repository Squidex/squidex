// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Infrastructure.Plugins;

namespace Squidex.Extensions.Actions.Webhook;

public sealed class WebhookPlugin : IPlugin
{
    public void ConfigureServices(IServiceCollection services, IConfiguration config)
    {
        services.AddHttpClient("WebhookPlugin", options =>
        {
            options.DefaultRequestHeaders.Add("User-Agent", "Squidex Webhook");
            options.DefaultRequestHeaders.Add("X-Application", "Squidex Webhook");
        });

        services.AddRuleAction<WebhookAction, WebhookActionHandler>();
    }
}
