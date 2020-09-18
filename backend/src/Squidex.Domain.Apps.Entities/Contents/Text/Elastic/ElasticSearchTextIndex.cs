// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Elasticsearch.Net;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.Text.Elastic
{
    [ExcludeFromCodeCoverage]
    public sealed class ElasticSearchTextIndex : ITextIndex
    {
        private readonly ElasticLowLevelClient client;
        private readonly string indexName;
        private readonly bool waitForTesting;

        public ElasticSearchTextIndex(string configurationString, string indexName, bool waitForTesting = false)
        {
            Guard.NotNull(configurationString, nameof(configurationString));
            Guard.NotNull(indexName, nameof(indexName));

            var config = new ConnectionConfiguration(new Uri(configurationString));

            client = new ElasticLowLevelClient(config);

            this.indexName = indexName;

            this.waitForTesting = waitForTesting;
        }

        public Task ClearAsync()
        {
            return Task.CompletedTask;
        }

        public async Task ExecuteAsync(params IndexCommand[] commands)
        {
            foreach (var command in commands)
            {
                switch (command)
                {
                    case UpsertIndexEntry upsert:
                        await UpsertAsync(upsert);
                        break;
                    case UpdateIndexEntry update:
                        await UpdateAsync(update);
                        break;
                    case DeleteIndexEntry delete:
                        await DeleteAsync(delete);
                        break;
                }
            }

            if (waitForTesting)
            {
                await Task.Delay(1000);
            }
        }

        private async Task UpsertAsync(UpsertIndexEntry upsert)
        {
            var data = new
            {
                appId = upsert.AppId.Id,
                appName = upsert.AppId.Name,
                contentId = upsert.ContentId,
                schemaId = upsert.SchemaId.Id,
                schemaName = upsert.SchemaId.Name,
                serveAll = upsert.ServeAll,
                servePublished = upsert.ServePublished,
                texts = upsert.Texts
            };

            var result = await client.IndexAsync<StringResponse>(indexName, upsert.DocId, CreatePost(data));

            if (!result.Success)
            {
                throw new InvalidOperationException($"Failed with ${result.Body}", result.OriginalException);
            }
        }

        private async Task UpdateAsync(UpdateIndexEntry update)
        {
            var data = new
            {
                doc = new
                {
                    serveAll = update.ServeAll,
                    servePublished = update.ServePublished
                }
            };

            var result = await client.UpdateAsync<StringResponse>(indexName, update.DocId, CreatePost(data));

            if (!result.Success)
            {
                throw new InvalidOperationException($"Failed with ${result.Body}", result.OriginalException);
            }
        }

        private Task DeleteAsync(DeleteIndexEntry delete)
        {
            return client.DeleteAsync<StringResponse>(indexName, delete.DocId);
        }

        private static PostData CreatePost<T>(T data)
        {
            return new SerializableData<T>(data);
        }

        public async Task<List<DomainId>?> SearchAsync(string? queryText, IAppEntity app, SearchFilter? filter, SearchScope scope)
        {
            var serveField = GetServeField(scope);

            var query = new
            {
                query = new
                {
                    @bool = new
                    {
                        must = new List<object>
                        {
                            new
                            {
                                term = new Dictionary<string, object>
                                {
                                    ["appId.keyword"] = app.Id
                                }
                            },
                            new
                            {
                                term = new Dictionary<string, string>
                                {
                                    [serveField] = "true"
                                }
                            },
                            new
                            {
                                multi_match = new
                                {
                                    fields = new[]
                                    {
                                        "texts.*"
                                    },
                                    query = queryText
                                }
                            }
                        },
                        should = new List<object>()
                    }
                },
                _source = new[]
                {
                    "contentId"
                },
                size = 2000
            };

            if (filter?.SchemaIds.Count > 0)
            {
                var bySchema = new
                {
                    terms = new Dictionary<string, object>
                    {
                        ["schemaId.keyword"] = filter.SchemaIds
                    }
                };

                if (filter.Must)
                {
                    query.query.@bool.must.Add(bySchema);
                }
                else
                {
                    query.query.@bool.should.Add(bySchema);
                }
            }

            var result = await client.SearchAsync<DynamicResponse>(indexName, CreatePost(query));

            if (!result.Success)
            {
                throw result.OriginalException;
            }

            var ids = new List<DomainId>();

            foreach (var item in result.Body.hits.hits)
            {
                if (item != null)
                {
                    ids.Add(item["_source"]["contentId"]);
                }
            }

            return ids;
        }

        private static string GetServeField(SearchScope scope)
        {
            return scope == SearchScope.Published ?
                "servePublished" :
                "serveAll";
        }
    }
}
