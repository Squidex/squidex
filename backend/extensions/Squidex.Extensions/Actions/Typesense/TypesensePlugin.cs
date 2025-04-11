// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Infrastructure.Plugins;

namespace Squidex.Extensions.Actions.Typesense;

public sealed class TypesensePlugin : IPlugin
{
    public void ConfigureServices(IServiceCollection services, IConfiguration config)
    {
        services.AddHttpClient("TypesenseAction");

        services.AddFlowStep<TypesenseFlowStep>();
#pragma warning disable CS0618 // Type or member is obsolete
        services.AddRuleAction<TypesenseAction>();
#pragma warning restore CS0618 // Type or member is obsolete
    }
}
