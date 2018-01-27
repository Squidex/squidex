// ==========================================================================
//  DefaultElasticClientFactory.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Elasticsearch.Net;

namespace Squidex.Infrastructure.ElasticSearch
{
    public class DefaultElasticClientFactory : IElasticLowLevelClientFactory
    {
        public IElasticLowLevelClient Create(Uri hostUrl)
        {
            Guard.NotNull(hostUrl, nameof(hostUrl));
            
            var settings = GetConfiguration(hostUrl);
            return new ElasticLowLevelClient(settings);
        }

        private static ConnectionConfiguration GetConfiguration(Uri hostUrl)
        {
            return new ConnectionConfiguration(hostUrl)
                .RequestTimeout(TimeSpan.FromSeconds(2));
        }
    }
}