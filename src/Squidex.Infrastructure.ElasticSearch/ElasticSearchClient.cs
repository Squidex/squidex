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
        private readonly IElasticLowLevelClient elasticClient;

        public ElasticSearchClient(IElasticLowLevelClient elasticClient)
        {
            Guard.NotNull(elasticClient, nameof(elasticClient));
            this.elasticClient = elasticClient;
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
    }
}