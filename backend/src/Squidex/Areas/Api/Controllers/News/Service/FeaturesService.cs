// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Squidex.Areas.Api.Controllers.News.Models;
using Squidex.ClientLibrary;

namespace Squidex.Areas.Api.Controllers.News.Service
{
    public sealed class FeaturesService
    {
        private const int FeatureVersion = 18;
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
                Version = FeatureVersion
            };

            if (client != null && version < FeatureVersion)
            {
                try
                {
                    var query = new ContentQuery
                    {
                        Filter = $"data/version/iv ge {FeatureVersion}"
                    };

                    var features = await client.GetAsync(query, flatten, ct);

                    result.Features = features.Items.Select(x => x.Data).ToList();
                }
                catch
                {
                    result.Features = new List<FeatureDto>();
                }
            }

            result.Features ??= new List<FeatureDto>();

            return result;
        }
    }
}
