﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Infrastructure.Plugins;

namespace Squidex.Extensions.Actions.SignalR;

public sealed class SignalRPlugin : IPlugin
{
    public void ConfigureServices(IServiceCollection services, IConfiguration config)
    {
        services.AddFlowStep<SignalRFlowStep>();
#pragma warning disable CS0618 // Type or member is obsolete
        services.AddRuleAction<SignalRAction>();
#pragma warning restore CS0618 // Type or member is obsolete
    }
}
