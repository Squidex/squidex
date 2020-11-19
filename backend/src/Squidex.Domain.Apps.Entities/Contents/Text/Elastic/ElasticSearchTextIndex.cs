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

        public Task InitializeAsync(CancellationToken ct = default)
        {
            return ElasticSearchMapping.ApplyAsync(client, indexName, ct);
        }

        public Task ClearAsync()
        {
            return Task.CompletedTask;
        }

        public async Task ExecuteAsync(params IndexCommand[] commands)
        {
            var args = new List<object>();

            foreach (var command in commands)
            {
                switch (command)
                {
                    case UpsertIndexEntry upsert:
                        Upsert(upsert, args);
                        break;
                    case UpdateIndexEntry update:
                        Update(update, args);
                        break;
                    case DeleteIndexEntry delete:
                        Delete(delete, args);
                        break;
                }
            }

            if (args.Count > 0)
            {
                var result = await client.BulkAsync<StringResponse>(PostData.MultiJson(args));

                if (!result.Success)
                {
                    throw new InvalidOperationException($"Failed with ${result.Body}", result.OriginalException);
                }
            }

            if (waitForTesting)
            {
                await Task.Delay(1000);
            }
        }

        private void Upsert(UpsertIndexEntry upsert, List<object> args)
        {
            args.Add(new
            {
                index = new
                {
                    _id = upsert.DocId,
                    _index = indexName,
                }
            });

            args.Add(new
            {
                appId = upsert.AppId.Id.ToString(),
                appName = upsert.AppId.Name,
                contentId = upsert.ContentId.ToString(),
                schemaId = upsert.SchemaId.Id.ToString(),
                schemaName = upsert.SchemaId.Name,
                serveAll = upsert.ServeAll,
                servePublished = upsert.ServePublished,
                texts = upsert.Texts
            });
        }

        private void Update(UpdateIndexEntry update, List<object> args)
        {
            args.Add(new
            {
                update = new
                {
                    _id = update.DocId,
                    _index = indexName,
                }
            });

            args.Add(new
            {
                doc = new
                {
                    serveAll = update.ServeAll,
                    servePublished = update.ServePublished
                }
            });
        }

        private void Delete(DeleteIndexEntry delete, List<object> args)
        {
            args.Add(new
            {
                delete = new
                {
                    _id = delete.DocId,
                    _index = indexName,
                }
            });
        }

        public async Task<List<DomainId>?> SearchAsync(string? queryText, IAppEntity app, SearchFilter? filter, SearchScope scope)
        {
            if (string.IsNullOrWhiteSpace(queryText))
            {
                return new List<DomainId>();
            }

            var isFuzzy = queryText.EndsWith("~", StringComparison.OrdinalIgnoreCase);

            if (isFuzzy)
            {
                queryText = queryText[..^1];
            }

            var field = "texts.*";

            if (queryText.Length >= 4 && queryText.IndexOf(":", StringComparison.OrdinalIgnoreCase) == 2)
            {
                var candidateLanguage = queryText.Substring(0, 2);

                if (Language.IsValidLanguage(candidateLanguage))
                {
                    field = $"texts.{candidateLanguage}";

                    queryText = queryText[3..];
                }
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
                                        field
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

        private static PostData CreatePost<T>(T data)
        {
            return new SerializableData<T>(data);
        }
    }
}
