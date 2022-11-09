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
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Plugins;

namespace Squidex.Extensions.Text.ElasticSearch;

public sealed class ElasticSearchTextPlugin : IPlugin
{
    public void ConfigureServices(IServiceCollection services, IConfiguration config)
    {
        var fullTextType = config.GetValue<string>("fullText:type");

        if (string.Equals(fullTextType, "elastic", StringComparison.OrdinalIgnoreCase))
        {
            var configuration = config.GetValue<string>("fullText:elastic:configuration");

            if (string.IsNullOrWhiteSpace(configuration))
            {
                var error = new ConfigurationError("Value is required.", "fullText:elastic:configuration");

                throw new ConfigurationException(error);
            }

            var indexName = config.GetValue<string>("fullText:elastic:indexName");

            if (string.IsNullOrWhiteSpace(indexName))
            {
                indexName = "squidex-index";
            }

            var openSearch = config.GetValue<bool>("fullText:elastic:openSearch");

            services.AddSingleton(c =>
            {
                IElasticSearchClient elasticSearchClient;

                if (openSearch)
                {
                    elasticSearchClient = new OpenSearchClient(configuration);
                }
                else
                {
                    elasticSearchClient = new ElasticSearchClient(configuration);
                }

                return new ElasticSearchTextIndex(elasticSearchClient, indexName, c.GetRequiredService<IJsonSerializer>());
            });

            services.AddSingleton<ITextIndex>(
                c => c.GetRequiredService<ElasticSearchTextIndex>());

            services.AddSingleton<IInitializable>(
                c => c.GetRequiredService<ElasticSearchTextIndex>());
        }
    }
}
