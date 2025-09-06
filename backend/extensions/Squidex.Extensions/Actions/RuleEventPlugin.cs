// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Domain.Apps.Entities.Backup;
using Squidex.Infrastructure.Plugins;

namespace Squidex.Extensions.Actions;

public sealed class RuleEventPlugin : IPlugin
{
    public void ConfigureServices(IServiceCollection services, IConfiguration config)
    {
        services.AddTransientAs<RuleEventMigrator>()
            .As<IEventMigrator>();
    }
}
