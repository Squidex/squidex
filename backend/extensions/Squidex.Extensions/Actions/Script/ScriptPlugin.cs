// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Infrastructure.Plugins;

namespace Squidex.Extensions.Actions.Script;

public sealed class ScriptPlugin : IPlugin
{
    public void ConfigureServices(IServiceCollection services, IConfiguration config)
    {
        services.AddFlowStep<ScriptFlowStep>();
#pragma warning disable CS0618 // Type or member is obsolete
        services.AddRuleAction<ScriptAction>();
#pragma warning restore CS0618 // Type or member is obsolete
    }
}
