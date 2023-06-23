// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Options;
using Squidex.ClientLibrary;
using FeatureDto = Squidex.Areas.Api.Controllers.News.Models.FeatureDto;
using FeaturesDto = Squidex.Areas.Api.Controllers.News.Models.FeaturesDto;

#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body

namespace Squidex.Areas.Api.Controllers.News.Service;

public sealed class FeaturesService
{
    private const int FeatureVersion = 21;
    private readonly QueryContext flatten = QueryContext.Default.Flatten();
    private readonly IContentsClient<NewsEntity, FeatureDto> contents;

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

            var client = new SquidexClient(squidexOptions);

            contents = client.Contents<NewsEntity, FeatureDto>("feature-news");
        }
    }

    public async Task<FeaturesDto> GetFeaturesAsync(int version = 0,
        CancellationToken ct = default)
    {
        var result = new FeaturesDto
        {
            Version = version
        };

        if (contents != null && version < FeatureVersion)
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

                var features = await contents.GetAsync(query, flatten, ct);

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
