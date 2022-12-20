// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Infrastructure.Plugins;

namespace Squidex.Extensions.Actions.Medium;

public sealed class MediumPlugin : IPlugin
{
    public void ConfigureServices(IServiceCollection services, IConfiguration config)
    {
        services.AddHttpClient("MediumAction", options =>
        {
            options.BaseAddress = new Uri("https://api.medium.com/");
            options.Timeout = TimeSpan.FromSeconds(4);
            options.DefaultRequestHeaders.Add("Accept", "application/json");
            options.DefaultRequestHeaders.Add("Accept-Charset", "utf-8");
            options.DefaultRequestHeaders.Add("User-Agent", "Squidex Headless CMS");
        });

        services.AddRuleAction<MediumAction, MediumActionHandler>();
    }
}
