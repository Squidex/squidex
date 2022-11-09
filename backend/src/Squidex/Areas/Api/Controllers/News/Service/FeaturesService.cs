// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Options;
using Squidex.Areas.Api.Controllers.News.Models;
using Squidex.ClientLibrary;

#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body

namespace Squidex.Areas.Api.Controllers.News.Service;

public sealed class FeaturesService
{
    private const int FeatureVersion = 21;
    private readonly QueryContext flatten = QueryContext.Default.Flatten();
    private readonly IContentsClient<NewsEntity, FeatureDto> client;

    public sealed class NewsEntity : Content<FeatureDto>
    {
    }

    public FeaturesService(IOptions<MyNewsOptions> options)
    {
        if (options.Value.IsConfigured())
        {
            var squidexOptions = new SquidexOptions
            {
                AppName = options.Value.AppName,
                ClientId = options.Value.ClientId,
                ClientSecret = options.Value.ClientSecret,
                Url = "https://cloud.squidex.io"
            };

            var clientManager = new SquidexClientManager(squidexOptions);

            client = clientManager.CreateContentsClient<NewsEntity, FeatureDto>("feature-news");
        }
    }

    public async Task<FeaturesDto> GetFeaturesAsync(int version = 0,
        CancellationToken ct = default)
    {
        var result = new FeaturesDto
        {
            Version = version
        };

        if (client != null && version < FeatureVersion)
        {
            try
            {
                var query = new ContentQuery();

                if (version == 0)
                {
                    query.Filter = $"data/version/iv eq {FeatureVersion}";
                }
                else
                {
                    query.Filter = $"data/version/iv le {FeatureVersion} and data/version/iv gt {version}";
                }

                var features = await client.GetAsync(query, flatten, ct);

                result.Features.AddRange(features.Items.Select(x => x.Data).ToList());

                if (features.Items.Count > 0)
                {
                    result.Version = features.Items.Max(x => x.Version);
                }
            }
            catch
            {
            }
        }

        return result;
    }
}
