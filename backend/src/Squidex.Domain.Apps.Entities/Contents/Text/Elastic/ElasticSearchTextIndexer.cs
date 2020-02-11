// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Elasticsearch.Net;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.Text.Elastic
{
    public sealed class ElasticSearchTextIndexer : ITextIndexer
    {
        private const string IndexName = "contents";
        private readonly ElasticLowLevelClient client;

        public ElasticSearchTextIndexer()
        {
            var config = new ConnectionConfiguration(new Uri("http://localhost:9200"));

            client = new ElasticLowLevelClient(config);
        }

        public async Task ExecuteAsync(NamedId<Guid> appId, NamedId<Guid> schemaId, params IIndexCommand[] commands)
        {
            foreach (var command in commands)
            {
                switch (command)
                {
                    case UpsertIndexEntry upsert:
                        await UpsertAsync(appId, schemaId, upsert);
                        break;
                    case UpdateIndexEntry update:
                        await UpdateAsync(update);
                        break;
                    case DeleteIndexEntry delete:
                        await DeleteAsync(delete);
                        break;
                }
            }
        }

        private async Task UpsertAsync(NamedId<Guid> appId, NamedId<Guid> schemaId, UpsertIndexEntry upsert)
        {
            upsert.Texts["en"] = "Foo";

            var data = new
            {
                appId = appId.Id,
                appName = appId.Name,
                contentId = upsert.ContentId,
                schemaId = schemaId.Id,
                schemaName = schemaId.Name,
                serveAll = upsert.ServeAll,
                servePublished = upsert.ServePublished,
                texts = upsert.Texts,
            };

            var result = await client.IndexAsync<StringResponse>(IndexName, upsert.DocId, CreatePost(data));

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
                    update.ServeAll,
                    update.ServePublished
                }
            };

            var result = await client.UpdateAsync<StringResponse>(IndexName, update.DocId, CreatePost(data));

            if (!result.Success)
            {
                throw new InvalidOperationException($"Failed with ${result.Body}", result.OriginalException);
            }
        }

        private Task DeleteAsync(DeleteIndexEntry delete)
        {
            return client.DeleteAsync<StringResponse>(IndexName, delete.DocId);
        }

        private static PostData CreatePost<T>(T data)
        {
            return new SerializableData<T>(data);
        }

        public async Task<List<Guid>?> SearchAsync(string? queryText, IAppEntity app, Guid schemaId, SearchScope scope = SearchScope.Published)
        {
            var serveField =
                scope == SearchScope.Published ?
                "servePublished" :
                "serveAll";

            var query = new
            {
                query = new
                {
                    @bool = new
                    {
                        must = new object[]
                        {
                            new
                            {
                                term = new Dictionary<string, string>
                                {
                                    ["appId.keyword"] = app.Id.ToString()
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
                        should = new object[]
                        {
                            new
                            {
                                term = new Dictionary<string, string>
                                {
                                    ["schemaId.keyword"] = schemaId.ToString()
                                }
                            }
                        }
                    }
                },
                _source = new[]
                {
                    "contentId"
                },
                size = 2000
            };

            var result = await client.SearchAsync<DynamicResponse>(IndexName, CreatePost(query));

            if (!result.Success)
            {
                throw result.OriginalException;
            }

            var ids = new List<Guid>();

            foreach (var item in result.Body.hits.hits)
            {
                if (item != null)
                {
                    ids.Add(Guid.Parse(item["_source"]["contentId"]));
                }
            }

            return ids;
        }
    }
}
