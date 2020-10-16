// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Elasticsearch.Net;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.Text.Elastic
{
    [ExcludeFromCodeCoverage]
    public sealed class ElasticSearchTextIndex : ITextIndex, IInitializable
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

        public async Task InitializeAsync(CancellationToken ct = default)
        {
            var query = new
            {
                properties = new Dictionary<string, object>
                {
                    ["texts.ar"] = new
                    {
                        type = "text",
                        analyzer = "arabic"
                    },
                    ["texts.hy"] = new
                    {
                        type = "text",
                        analyzer = "armenian"
                    },
                    ["texts.eu"] = new
                    {
                        type = "text",
                        analyzer = "basque"
                    },
                    ["texts.bn"] = new
                    {
                        type = "text",
                        analyzer = "bengali"
                    },
                    ["texts.br"] = new
                    {
                        type = "text",
                        analyzer = "brazilian"
                    },
                    ["texts.bg"] = new
                    {
                        type = "text",
                        analyzer = "bulgarian"
                    },
                    ["texts.ca"] = new
                    {
                        type = "text",
                        analyzer = "catalan"
                    },
                    ["texts.zh"] = new
                    {
                        type = "text",
                        analyzer = "cjk"
                    },
                    ["texts.ja"] = new
                    {
                        type = "text",
                        analyzer = "cjk"
                    },
                    ["texts.ko"] = new
                    {
                        type = "text",
                        analyzer = "cjk"
                    },
                    ["texts.cs"] = new
                    {
                        type = "text",
                        analyzer = "czech"
                    },
                    ["texts.da"] = new
                    {
                        type = "text",
                        analyzer = "danish"
                    },
                    ["texts.nl"] = new
                    {
                        type = "text",
                        analyzer = "dutch"
                    },
                    ["texts.en"] = new
                    {
                        type = "text",
                        analyzer = "english"
                    },
                    ["texts.fi"] = new
                    {
                        type = "text",
                        analyzer = "finnish"
                    },
                    ["texts.fr"] = new
                    {
                        type = "text",
                        analyzer = "french"
                    },
                    ["texts.gl"] = new
                    {
                        type = "text",
                        analyzer = "galician"
                    },
                    ["texts.de"] = new
                    {
                        type = "text",
                        analyzer = "german"
                    },
                    ["texts.el"] = new
                    {
                        type = "text",
                        analyzer = "greek"
                    },
                    ["texts.hi"] = new
                    {
                        type = "text",
                        analyzer = "hindi"
                    },
                    ["texts.hu"] = new
                    {
                        type = "text",
                        analyzer = "hungarian"
                    },
                    ["texts.id"] = new
                    {
                        type = "text",
                        analyzer = "indonesian"
                    },
                    ["texts.ga"] = new
                    {
                        type = "text",
                        analyzer = "irish"
                    },
                    ["texts.it"] = new
                    {
                        type = "text",
                        analyzer = "italian"
                    },
                    ["texts.lv"] = new
                    {
                        type = "text",
                        analyzer = "latvian"
                    },
                    ["texts.lt"] = new
                    {
                        type = "text",
                        analyzer = "lithuanian"
                    },
                    ["texts.nb"] = new
                    {
                        type = "text",
                        analyzer = "norwegian"
                    },
                    ["texts.nn"] = new
                    {
                        type = "text",
                        analyzer = "norwegian"
                    },
                    ["texts.no"] = new
                    {
                        type = "text",
                        analyzer = "norwegian"
                    },
                    ["texts.pt"] = new
                    {
                        type = "text",
                        analyzer = "portuguese"
                    },
                    ["texts.ro"] = new
                    {
                        type = "text",
                        analyzer = "romanian"
                    },
                    ["texts.ru"] = new
                    {
                        type = "text",
                        analyzer = "russian"
                    },
                    ["texts.ku"] = new
                    {
                        type = "text",
                        analyzer = "sorani"
                    },
                    ["texts.es"] = new
                    {
                        type = "text",
                        analyzer = "spanish"
                    },
                    ["texts.sv"] = new
                    {
                        type = "text",
                        analyzer = "swedish"
                    },
                    ["texts.tr"] = new
                    {
                        type = "text",
                        analyzer = "turkish"
                    },
                    ["texts.th"] = new
                    {
                        type = "text",
                        analyzer = "thai"
                    }
                }
            };

            var result = await client.Indices.PutMappingAsync<StringResponse>(indexName, CreatePost(query));

            if (!result.Success)
            {
                throw new InvalidOperationException($"Failed with ${result.Body}", result.OriginalException);
            }
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
                appId = upsert.AppId.Id.ToString(),
                appName = upsert.AppId.Name,
                contentId = upsert.ContentId.ToString(),
                schemaId = upsert.SchemaId.Id.ToString(),
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
            if (string.IsNullOrWhiteSpace(queryText))
            {
                return new List<DomainId>();
            }

            var isFuzzy = queryText.StartsWith("~", StringComparison.OrdinalIgnoreCase);

            if (isFuzzy)
            {
                queryText = queryText.Substring(1);
            }

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
                                    fuzziness = isFuzzy ? (object)"AUTO" : 0,
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
                        ["schemaId.keyword"] = filter.SchemaIds.Select(x => x.ToString()).ToArray()
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
                    ids.Add(DomainId.Create(item["_source"]["contentId"]));
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
