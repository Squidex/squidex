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
using Squidex.Hosting;
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
            var config = new ConnectionConfiguration(new Uri(configurationString));

            client = new ElasticLowLevelClient(config);

            this.indexName = indexName;

            this.waitForTesting = waitForTesting;
        }

        public Task InitializeAsync(
            CancellationToken ct)
        {
            return ElasticSearchMapping.ApplyAsync(client, indexName, ct);
        }

        public Task ClearAsync(
            CancellationToken ct = default)
        {
            return Task.CompletedTask;
        }

        public async Task ExecuteAsync(IndexCommand[] commands,
            CancellationToken ct = default)
        {
            var args = new List<object>();

            foreach (var command in commands)
            {
                CommandFactory.CreateCommands(command, args, indexName);
            }

            if (args.Count > 0)
            {
                var result = await client.BulkAsync<StringResponse>(PostData.MultiJson(args), ctx: ct);

                if (!result.Success)
                {
                    throw new InvalidOperationException($"Failed with ${result.Body}", result.OriginalException);
                }
            }

            if (waitForTesting)
            {
                await Task.Delay(1000, ct);
            }
        }

        public Task<List<DomainId>?> SearchAsync(IAppEntity app, GeoQuery query, SearchScope scope,
            CancellationToken ct = default)
        {
            return Task.FromResult<List<DomainId>?>(null);
        }

        public async Task<List<DomainId>?> SearchAsync(IAppEntity app, TextQuery query, SearchScope scope,
            CancellationToken ct = default)
        {
            Guard.NotNull(app, nameof(app));
            Guard.NotNull(query, nameof(query));

            var queryText = query.Text;

            if (string.IsNullOrWhiteSpace(queryText))
            {
                return null;
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

            var elasticQuery = new
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
                                    query = query.Text
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

            if (query.Filter?.SchemaIds?.Length > 0)
            {
                var bySchema = new
                {
                    terms = new Dictionary<string, object>
                    {
                        ["schemaId.keyword"] = query.Filter.SchemaIds.Select(x => x.ToString()).ToArray()
                    }
                };

                if (query.Filter.Must)
                {
                    elasticQuery.query.@bool.must.Add(bySchema);
                }
                else
                {
                    elasticQuery.query.@bool.should.Add(bySchema);
                }
            }

            var result = await client.SearchAsync<DynamicResponse>(indexName, CreatePost(elasticQuery), ctx: ct);

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
