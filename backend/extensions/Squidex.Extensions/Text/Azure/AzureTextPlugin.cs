// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Domain.Apps.Entities.Contents.Text;
using Squidex.Hosting;
using Squidex.Hosting.Configuration;
using Squidex.Infrastructure.Plugins;

namespace Squidex.Extensions.Text.Azure;

public sealed class AzureTextPlugin : IPlugin
{
    public void ConfigureServices(IServiceCollection services, IConfiguration config)
    {
        var fullTextType = config.GetValue<string>("fullText:type");

        if (string.Equals(fullTextType, "Azure", StringComparison.OrdinalIgnoreCase))
        {
            var serviceEndpoint = config.GetValue<string>("fullText:azure:serviceEndpoint");

            if (string.IsNullOrWhiteSpace(serviceEndpoint))
            {
                var error = new ConfigurationError("Value is required.", "fullText:azure:serviceEndpoint");

                throw new ConfigurationException(error);
            }

            var serviceApiKey = config.GetValue<string>("fullText:azure:apiKey");

            if (string.IsNullOrWhiteSpace(serviceApiKey))
            {
                var error = new ConfigurationError("Value is required.", "fullText:azure:apiKey");

                throw new ConfigurationException(error);
            }

            var indexName = config.GetValue<string>("fullText:azure:indexName");

            if (string.IsNullOrWhiteSpace(indexName))
            {
                indexName = "squidex-index";
            }

            services.AddSingleton(
                c => new AzureTextIndex(serviceEndpoint, serviceApiKey, indexName));

            services.AddSingleton<ITextIndex>(
                c => c.GetRequiredService<AzureTextIndex>());

            services.AddSingleton<IInitializable>(
                c => c.GetRequiredService<AzureTextIndex>());
        }
    }
}
