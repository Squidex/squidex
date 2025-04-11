// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Infrastructure.Plugins;

namespace Squidex.Extensions.Actions.Notification;

public sealed class NotificationPlugin : IPlugin
{
    public void ConfigureServices(IServiceCollection services, IConfiguration config)
    {
        services.AddFlowStep<NotificationFlowStep>();
#pragma warning disable CS0618 // Type or member is obsolete
        services.AddRuleAction<NotificationAction>();
#pragma warning restore CS0618 // Type or member is obsolete
    }
}
