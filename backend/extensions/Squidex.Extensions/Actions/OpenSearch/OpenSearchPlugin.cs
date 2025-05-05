// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Infrastructure.Plugins;

namespace Squidex.Extensions.Actions.OpenSearch;

public sealed class OpenSearchPlugin : IPlugin
{
    public void ConfigureServices(IServiceCollection services, IConfiguration config)
    {
        services.AddFlowStep<OpenSearchFlowStep>();
#pragma warning disable CS0618 // Type or member is obsolete
        services.AddRuleAction<OpenSearchAction>();
#pragma warning restore CS0618 // Type or member is obsolete
    }
}
