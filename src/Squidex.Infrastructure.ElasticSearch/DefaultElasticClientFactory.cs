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

        public IElasticLowLevelClient Create(Uri hostUrl, string username, string password)
        {
            Guard.NotNull(hostUrl, nameof(hostUrl));
            Guard.NotNullOrEmpty(username, nameof(username));
            Guard.NotNullOrEmpty(password, nameof(password));

            var settings = GetConfiguration(hostUrl, username, password, true);
            return new ElasticLowLevelClient(settings);
        }

        private static ConnectionConfiguration GetConfiguration(Uri hostUrl, string username = "", string password = "",
            bool requiresAuth = false)
        {
            ConnectionConfiguration result;

            if (requiresAuth)
            {
                result = new ConnectionConfiguration(hostUrl)
                    .RequestTimeout(TimeSpan.FromSeconds(2))
                    .BasicAuthentication(username, password);
            }
            else
            {
                result = new ConnectionConfiguration(hostUrl)
                    .RequestTimeout(TimeSpan.FromSeconds(2));
            }

            return result;
        }
    }
}