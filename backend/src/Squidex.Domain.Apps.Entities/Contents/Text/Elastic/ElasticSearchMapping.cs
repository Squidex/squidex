// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Elasticsearch.Net;

namespace Squidex.Domain.Apps.Entities.Contents.Text.Elastic
{
    public static class ElasticSearchMapping
    {
        public static async Task ApplyAsync(IElasticLowLevelClient elastic, string indexName,
            CancellationToken ct = default)
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

            var result = await elastic.Indices.PutMappingAsync<StringResponse>(indexName, CreatePost(query), ctx: ct);

            if (!result.Success)
            {
                throw new InvalidOperationException($"Failed with ${result.Body}", result.OriginalException);
            }
        }

        private static PostData CreatePost<T>(T data)
        {
            return new SerializableData<T>(data);
        }
    }
}
