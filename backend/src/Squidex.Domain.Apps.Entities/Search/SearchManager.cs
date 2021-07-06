﻿// ==========================================================================
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
using Squidex.Log;

namespace Squidex.Domain.Apps.Entities.Search
{
    public sealed class SearchManager : ISearchManager
    {
        private static readonly SearchResults Empty = new SearchResults();
        private readonly IEnumerable<ISearchSource> searchSources;
        private readonly ISemanticLog log;

        public SearchManager(IEnumerable<ISearchSource> searchSources, ISemanticLog log)
        {
            this.searchSources = searchSources;

            this.log = log;
        }

        public async Task<SearchResults> SearchAsync(string? query, Context context,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 3)
            {
                return new SearchResults();
            }

            var tasks = searchSources.Select(x => SearchAsync(x, query, context, ct));

            var results = await Task.WhenAll(tasks);

            return new SearchResults(results.SelectMany(x => x));
        }

        private async Task<SearchResults> SearchAsync(ISearchSource source, string query, Context context,
            CancellationToken ct)
        {
            try
            {
                return await source.SearchAsync(query, context, ct);
            }
            catch (Exception ex)
            {
                log.LogError(ex, query, (c, w) => w
                    .WriteProperty("operation", "search")
                    .WriteProperty("status", "Failed")
                    .WriteProperty("query", query));

                return Empty;
            }
        }
    }
}
