// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.RegularExpressions;
using Elasticsearch.Net;
using Newtonsoft.Json;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Contents.Text;
using Squidex.Hosting;
using Squidex.Infrastructure;

namespace Squidex.Extensions.Text.ElasticSearch
{
    public sealed class ElasticSearchTextIndex : ITextIndex, IInitializable
    {
        private static readonly Regex LanguageRegex = new Regex(@"[^\w]+([a-z\-_]{2,}):", RegexOptions.Compiled | RegexOptions.ExplicitCapture);
        private static readonly Regex LanguageRegexStart = new Regex(@"$^([a-z\-_]{2,}):", RegexOptions.Compiled | RegexOptions.ExplicitCapture);
        private readonly ElasticLowLevelClient client;
        private readonly QueryParser queryParser = new QueryParser(ElasticSearchIndexDefinition.GetFieldPath);
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
            return ElasticSearchIndexDefinition.ApplyAsync(client, indexName, ct);
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

        public async Task<List<DomainId>> SearchAsync(IAppEntity app, GeoQuery query, SearchScope scope,
            CancellationToken ct = default)
        {
            Guard.NotNull(app);
            Guard.NotNull(query);

            var serveField = GetServeField(scope);

            var elasticQuery = new
            {
                query = new
                {
                    @bool = new
                    {
                        filter = new object[]
                        {
                            new
                            {
                                term = new Dictionary<string, object>
                                {
                                    ["schemaId.keyword"] = query.SchemaId.ToString()
                                }
                            },
                            new
                            {
                                term = new Dictionary<string, string>
                                {
                                    ["geoField.keyword"] = query.Field
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
                                geo_distance = new
                                {
                                    geoObject = new
                                    {
                                        lat = query.Latitude,
                                        lon = query.Longitude
                                    },
                                    distance = $"{query.Radius}m"
                                }
                            }
                        },
                    }
                },
                _source = new[]
                {
                    "contentId"
                },
                size = query.Take
            };

            return await SearchAsync(elasticQuery, ct);
        }

        public async Task<List<DomainId>> SearchAsync(IAppEntity app, TextQuery query, SearchScope scope,
            CancellationToken ct = default)
        {
            Guard.NotNull(app);
            Guard.NotNull(query);

            var parsed = queryParser.Parse(query.Text);

            if (parsed == null)
            {
                return null;
            }

            var serveField = GetServeField(scope);

            var elasticQuery = new
            {
                query = new
                {
                    @bool = new
                    {
                        filter = new List<object>
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
                            }
                        },
                        must = new
                        {
                            query_string = new
                            {
                                query = parsed.Text
                            }
                        },
                        should = new List<object>()
                    }
                },
                _source = new[]
                {
                    "contentId"
                },
                size = query.Take
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

                elasticQuery.query.@bool.filter.Add(bySchema);
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

            var json = JsonConvert.SerializeObject(elasticQuery, Formatting.Indented);

            return await SearchAsync(elasticQuery, ct);
        }

        private async Task<List<DomainId>> SearchAsync(object query,
            CancellationToken ct)
        {
            var result = await client.SearchAsync<DynamicResponse>(indexName, CreatePost(query), ctx: ct);

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
