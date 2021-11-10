// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Squidex.Hosting.Configuration;

namespace Squidex.Domain.Apps.Entities.MongoDb.FullText
{
    public static class AtlasIndexDefinition
    {
        private static readonly Dictionary<string, string> Fields = new Dictionary<string, string>();
        private static readonly Dictionary<string, string> Analyzers = new Dictionary<string, string>
        {
            ["iv"] = "lucene.standard",
            ["ar"] = "lucene.arabic",
            ["hy"] = "lucene.armenian",
            ["eu"] = "lucene.basque",
            ["bn"] = "lucene.bengali",
            ["br"] = "lucene.brazilian",
            ["bg"] = "lucene.bulgarian",
            ["ca"] = "lucene.catalan",
            ["ko"] = "lucene.cjk",
            ["jp"] = "lucene.cjk",
            ["zh"] = "lucene.cjk",
            ["da"] = "lucene.danish",
            ["en"] = "lucene.english",
            ["fi"] = "lucene.finnish",
            ["fr"] = "lucene.french",
            ["el"] = "lucene.greek",
            ["hi"] = "lucene.hindi",
            ["id"] = "lucene.indonesian",
            ["ga"] = "lucene.irish",
            ["it"] = "lucene.italian",
            ["lv"] = "lucene.latvian",
            ["no"] = "lucene.norwegian",
            ["pt"] = "lucene.portuguese",
            ["ro"] = "lucene.romanian",
            ["ru"] = "lucene.russian",
            ["es"] = "lucene.spanish",
            ["sv"] = "lucene.swedish",
            ["tr"] = "lucene.turkish",
            ["uk"] = "lucene.ukrainian"
        };

        public sealed class ErrorResponse
        {
            public string Detail { get; set; }

            public string ErrorCode { get; set; }
        }

        static AtlasIndexDefinition()
        {
            Fields = Analyzers.ToDictionary(x => x.Key, x => x.Key);
        }

        public static string GetTextField(string key)
        {
            if (Fields.TryGetValue(key, out var fieldName))
            {
                return fieldName;
            }

            if (key.Length > 2 && Analyzers.TryGetValue(key[2..], out fieldName))
            {
                return fieldName;
            }

            return "t.iv";
        }

        public static async Task<string> CreateIndexAsync(AtlasOptions options,
            string database,
            string collectionName,
            CancellationToken ct)
        {
            var (index, name) = Create(database, collectionName);

            using (var httpClient = new HttpClient(new HttpClientHandler
            {
                Credentials = new NetworkCredential(options.PublicKey, options.PrivateKey, "cloud.mongodb.com")
            }))
            {
                var url = $"https://cloud.mongodb.com/api/atlas/v1.0/groups/{options.GroupId}/clusters/{options.ClusterName}/fts/indexes";

                var result = await httpClient.PostAsJsonAsync(url, index, ct);

                if (result.IsSuccessStatusCode)
                {
                    return name;
                }

                var error = await result.Content.ReadFromJsonAsync<ErrorResponse>(cancellationToken: ct);

                if (error?.ErrorCode != "ATLAS_FTS_DUPLICATE_INDEX")
                {
                    var message = new ConfigurationError($"Creating index failed with {result.StatusCode}: {error?.Detail}");

                    throw new ConfigurationException(message);
                }
            }

            return name;
        }

        public static (object, string) Create(string database, string collectionName)
        {
            var name = $"{database}_{collectionName}_text".ToLowerInvariant();

            var texts = new
            {
                type = "document",
                fields = new Dictionary<string, object>(),
                dynamic = false
            };

            var index = new
            {
                collectionName,
                database,
                name,
                mappings = new
                {
                    dynamic = false,
                    fields = new Dictionary<string, object>
                    {
                        ["_ai"] = new
                        {
                            type = "string",
                            analyzer = "lucene.keyword"
                        },
                        ["_si"] = new
                        {
                            type = "string",
                            analyzer = "lucene.keyword"
                        },
                        ["_ci"] = new
                        {
                            type = "string",
                            analyzer = "lucene.keyword"
                        },
                        ["fa"] = new
                        {
                            type = "boolean"
                        },
                        ["fp"] = new
                        {
                            type = "boolean"
                        },
                        ["t"] = texts
                    }
                }
            };

            foreach (var (field, analyzer) in Analyzers)
            {
                texts.fields[field] = new
                {
                    type = "string",
                    analyzer,
                    searchAnalyzer = analyzer,
                    store = false
                };
            }

            return (index, name);
        }
    }
}
