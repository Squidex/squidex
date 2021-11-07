// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Elasticsearch.Net;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Contents.Text;
using Squidex.Hosting;
using Squidex.Infrastructure;

namespace Squidex.Extensions.Text.ElasticSearch
{
    public sealed class ElasticSearchTextIndex : ITextIndex, IInitializable
    {
        private readonly ElasticLowLevelClient client;
        private readonly string indexName;

        public ElasticSearchTextIndex(string configurationString, string indexName)
        {
            var config = new ConnectionConfiguration(new Uri(configurationString));

            client = new ElasticLowLevelClient(config);

            this.indexName = indexName;
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

            if (args.Count == 0)
            {
                return;
            }

            var result = await client.BulkAsync<StringResponse>(PostData.MultiJson(args), ctx: ct);

            if (!result.Success)
            {
                throw new InvalidOperationException($"Failed with ${result.Body}", result.OriginalException);
            }
        }

        public Task<List<DomainId>> SearchAsync(IAppEntity app, GeoQuery query, SearchScope scope,
            CancellationToken ct = default)
        {
            return Task.FromResult<List<DomainId>>(null);
        }

        public async Task<List<DomainId>> SearchAsync(IAppEntity app, TextQuery query, SearchScope scope,
            CancellationToken ct = default)
        {
            Guard.NotNull(app, nameof(app));
            Guard.NotNull(query, nameof(query));

            var (text, take) = query;

            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            var isFuzzy = text.EndsWith("~", StringComparison.OrdinalIgnoreCase);

            if (isFuzzy)
            {
                text = text[..^1];
            }

            var field = "texts.*";

            if (text.Length >= 4 && text.IndexOf(":", StringComparison.OrdinalIgnoreCase) == 2)
            {
                var candidateLanguage = text.Substring(0, 2);

                if (Language.IsValidLanguage(candidateLanguage))
                {
                    field = $"texts.{candidateLanguage}";

                    text = text[3..];
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
                                    query = text
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
                size = take
            };

            if (query.RequiredSchemaIds?.Count > 0)
            {
                var bySchema = new
                {
                    terms = new Dictionary<string, object>
                    {
                        ["schemaId.keyword"] = query.RequiredSchemaIds.Select(x => x.ToString()).ToArray()
                    }
                };

                elasticQuery.query.@bool.must.Add(bySchema);
            }
            else if (query.PreferredSchemaId.HasValue)
            {
                var bySchema = new
                {
                    terms = new Dictionary<string, object>
                    {
                        ["schemaId.keyword"] = query.PreferredSchemaId.ToString()
                    }
                };

                elasticQuery.query.@bool.should.Add(bySchema);
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
