// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.RegularExpressions;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Contents.Text;
using Squidex.Hosting;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json;

namespace Squidex.Extensions.Text.ElasticSearch;

public sealed class ElasticSearchTextIndex : ITextIndex, IInitializable
{
    private static readonly Regex LanguageRegex = new Regex(@"[^\w]+([a-z\-_]{2,}):", RegexOptions.Compiled | RegexOptions.ExplicitCapture);
    private static readonly Regex LanguageRegexStart = new Regex(@"$^([a-z\-_]{2,}):", RegexOptions.Compiled | RegexOptions.ExplicitCapture);
    private readonly IJsonSerializer jsonSerializer;
    private readonly IElasticSearchClient elasticClient;
    private readonly QueryParser queryParser = new QueryParser(ElasticSearchIndexDefinition.GetFieldPath);
    private readonly string indexName;

    public ElasticSearchTextIndex(IElasticSearchClient elasticClient, string indexName, IJsonSerializer jsonSerializer)
    {
        this.elasticClient = elasticClient;
        this.indexName = indexName;
        this.jsonSerializer = jsonSerializer;
    }

    public Task InitializeAsync(
        CancellationToken ct)
    {
        return ElasticSearchIndexDefinition.ApplyAsync(elasticClient, indexName, ct);
    }

    public Task ClearAsync(
        CancellationToken ct = default)
    {
        return Task.CompletedTask;
    }

    public Task ExecuteAsync(IndexCommand[] commands,
        CancellationToken ct = default)
    {
        var args = new List<object>();

        foreach (var command in commands)
        {
            CommandFactory.CreateCommands(command, args, indexName);
        }

        if (args.Count == 0)
        {
            return Task.CompletedTask;
        }

        return elasticClient.BulkAsync(args, ct);
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
                    }
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

        var json = jsonSerializer.Serialize(elasticQuery, true);

        return await SearchAsync(elasticQuery, ct);
    }

    private async Task<List<DomainId>> SearchAsync(object query,
        CancellationToken ct)
    {
        var hits = await elasticClient.SearchAsync(indexName, query, ct);

        var ids = new List<DomainId>();

        foreach (var item in hits)
        {
             ids.Add(DomainId.Create(item["_source"]["contentId"]));
        }

        return ids;
    }

    private static string GetServeField(SearchScope scope)
    {
        return scope == SearchScope.Published ? "servePublished" : "serveAll";
    }
}
