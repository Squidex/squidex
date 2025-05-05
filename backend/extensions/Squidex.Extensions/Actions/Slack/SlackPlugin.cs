// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Migrations.OldActions;
using Squidex.Infrastructure.Plugins;

namespace Squidex.Extensions.Actions.Slack;

public sealed class SlackPlugin : IPlugin
{
    public void ConfigureServices(IServiceCollection services, IConfiguration config)
    {
        services.AddHttpClient("SlackAction", options =>
        {
            options.Timeout = TimeSpan.FromSeconds(2);
        });

        services.AddFlowStep<SlackFlowStep>();
#pragma warning disable CS0618 // Type or member is obsolete
        services.AddRuleAction<SlackAction>();
#pragma warning restore CS0618 // Type or member is obsolete
    }
}
