// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Infrastructure.Plugins;

namespace Squidex.Extensions.Assets.Azure;

public sealed class AzureMetadataSourcePlugin : IPlugin
{
    public void ConfigureServices(IServiceCollection services, IConfiguration config)
    {
        var options = config.GetSection("assets:azurecognitive").Get<AzureMetadataSourceOptions>() ?? new ();

        if (options.IsConfigured())
        {
            services.AddSingleton<IAssetMetadataSource, AzureMetadataSource>();
            services.AddSingleton(Options.Create(options));
        }
    }
}
