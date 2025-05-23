﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Infrastructure.Plugins;

namespace Squidex.Extensions.Actions.Fastly;

public sealed class FastlyPlugin : IPlugin
{
    public void ConfigureServices(IServiceCollection services, IConfiguration config)
    {
        services.AddHttpClient("Fastly", options =>
        {
            options.BaseAddress = new Uri("https://api.fastly.com");
            options.Timeout = TimeSpan.FromSeconds(2);
        });

        services.AddFlowStep<FastlyFlowStep>();
#pragma warning disable CS0618 // Type or member is obsolete
        services.AddRuleAction<FastlyAction>();
#pragma warning restore CS0618 // Type or member is obsolete
    }
}
