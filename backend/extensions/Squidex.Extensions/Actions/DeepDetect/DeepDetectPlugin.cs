// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Infrastructure.Plugins;

namespace Squidex.Extensions.Actions.DeepDetect;

internal sealed class DeepDetectPlugin : IPlugin
{
    public void ConfigureServices(IServiceCollection services, IConfiguration config)
    {
        var url = config.GetValue<string>("deepdetect:url");

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return;
        }

        services.AddHttpClient("DeepDetect", client =>
        {
            client.BaseAddress = uri;
        });

        services.AddRuleAction<DeepDetectAction, DeepDetectActionHandler>();
    }
}
