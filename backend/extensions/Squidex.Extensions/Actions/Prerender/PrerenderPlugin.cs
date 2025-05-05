// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Infrastructure.Plugins;

namespace Squidex.Extensions.Actions.Prerender;

public sealed class PrerenderPlugin : IPlugin
{
    public void ConfigureServices(IServiceCollection services, IConfiguration config)
    {
        services.AddHttpClient("Prerender", options =>
        {
            options.BaseAddress = new Uri("https://api.prerender.io");
        });

        services.AddFlowStep<PrerenderFlowStep>();
#pragma warning disable CS0618 // Type or member is obsolete
        services.AddRuleAction<PrerenderAction>();
#pragma warning restore CS0618 // Type or member is obsolete
    }
}
