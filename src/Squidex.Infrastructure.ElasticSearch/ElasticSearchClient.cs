// ==========================================================================
//  ElasticSearchClient.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.IO;
using System.Threading.Tasks;
using Elasticsearch.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Squidex.Infrastructure.SearchEngines;

namespace Squidex.Infrastructure.ElasticSearch
{
    public class ElasticSearchClient : ISearchEngine
    {
        private IElasticLowLevelClient elasticClient;
        private IElasticLowLevelClientFactory clientFactory;

        public ElasticSearchClient(IElasticLowLevelClientFactory clientFactory)
        {
            Guard.NotNull(clientFactory, nameof(clientFactory));
            this.clientFactory = clientFactory;
        }
        
        public async Task<bool> AddContentToIndexAsync(JObject content, Guid contentId, string typeName, string indexName)
        {
            var response = await elasticClient.IndexAsync<StringResponse>(indexName, typeName, contentId.ToString(), PostData.Serializable(content));
            return response.HttpStatusCode == 201 || response.HttpStatusCode == 200;
        }

        public async Task<bool> UpdateContentInIndexAsync(JObject content, Guid contentId, string typeName, string indexName)
        {
            var response = await elasticClient.IndexAsync<StringResponse>(indexName, typeName, contentId.ToString(), PostData.Serializable(content));
            return response.HttpStatusCode == 200;
        }

        public async Task<bool> DeleteContentFromIndexAsync(Guid contentId, string typeName, string indexName)
        {
            var response = await elasticClient.DeleteAsync<StringResponse>(indexName, typeName, contentId.ToString());
            return response.HttpStatusCode == 200;
        }

        public bool Connect(string hostUrl)
        {
            return ConnectCore(hostUrl, null, null);
        }

        public bool Connect(string hostUrl, string username, string password)
        {
            return ConnectCore(hostUrl, username, password);
        }

        private bool ConnectCore(string hostUrl, string username, string password)
        {
            Guard.NotNullOrEmpty(hostUrl, nameof(hostUrl));
            var result = false;
            
            if (Uri.TryCreate(hostUrl, UriKind.Absolute, out var hostParsedUri))
            {
                if (string.IsNullOrEmpty(username) && string.IsNullOrEmpty(password))
                {
                    elasticClient = clientFactory.Create(hostParsedUri);
                }
                else
                {
                    elasticClient = clientFactory.Create(hostParsedUri, username, password);
                }
                
                result = true;
            }

            return result;
        }
    }
}