// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Squidex.Areas.Api.Controllers.News.Models;
using Squidex.ClientLibrary;

namespace Squidex.Areas.Api.Controllers.News.Service
{
    public sealed class FeaturesService
    {
        private const string AppName = "squidex-website";
        private const string ClientId = "squidex-website:default";
        private const string ClientSecret = "QGgqxd7bDHBTEkpC6fj8sbdPWgZrPrPfr3xzb3LKoec=";
        private const int FeatureVersion = 1;
        private static readonly QueryContext Flatten = QueryContext.Default.Flatten();
        private readonly SquidexClient<NewsEntity, FeatureDto> client;

        public sealed class NewsEntity : SquidexEntityBase<FeatureDto>
        {
        }

        public FeaturesService()
        {
            var clientManager = new SquidexClientManager("https://cloud.squidex.io", AppName, ClientId, ClientSecret);

            client = clientManager.GetClient<NewsEntity, FeatureDto>("feature-news");
        }

        public async Task<FeaturesDto> GetFeaturesAsync(int version = 0)
        {
            var result = new FeaturesDto { Features = new List<FeatureDto>(), Version = FeatureVersion };

            if (version < FeatureVersion)
            {
                var entities = await client.GetAsync(filter: $"data/version/iv ge ${version}", context: Flatten);

                result.Features.AddRange(entities.Items.Select(x => x.Data));
            }

            return result;
        }
    }
}
