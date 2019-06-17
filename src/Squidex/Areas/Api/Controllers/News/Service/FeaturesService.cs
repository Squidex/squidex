// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Squidex.Areas.Api.Controllers.News.Models;
using Squidex.ClientLibrary;

namespace Squidex.Areas.Api.Controllers.News.Service
{
    public sealed class FeaturesService
    {
        private const int FeatureVersion = 3;
        private static readonly QueryContext Flatten = QueryContext.Default.Flatten();
        private readonly SquidexClient<NewsEntity, FeatureDto> client;

        public sealed class NewsEntity : SquidexEntityBase<FeatureDto>
        {
        }

        public FeaturesService(IOptions<MyNewsOptions> options)
        {
            if (options.Value.IsConfigured())
            {
                var clientManager = new SquidexClientManager("https://cloud.squidex.io",
                    options.Value.AppName,
                    options.Value.ClientId,
                    options.Value.ClientSecret);

                client = clientManager.GetClient<NewsEntity, FeatureDto>("feature-news");
            }
        }

        public async Task<FeaturesDto> GetFeaturesAsync(int version = 0)
        {
            var result = new FeaturesDto { Features = new List<FeatureDto>(), Version = FeatureVersion };

            if (client != null && version < FeatureVersion)
            {
                var features = await client.GetAsync(filter: $"data/version/iv gt {version}", context: Flatten);

                result.Features.AddRange(features.Items.Select(x => x.Data));
            }

            return result;
        }
    }
}
