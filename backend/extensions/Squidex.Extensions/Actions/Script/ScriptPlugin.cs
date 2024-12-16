// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Flows;
using Squidex.Infrastructure.Plugins;

namespace Squidex.Extensions.Actions.Script;

public sealed class ScriptPlugin : IPlugin
{
    public void ConfigureServices(IServiceCollection services, IConfiguration config)
    {
        services.AddFlowStep<ScriptStep>();
        services.Configure<FlowOptions>(options =>
        {
            options.Steps.Remove(typeof(Flows.Steps.ScriptStep));
        });
    }
}
