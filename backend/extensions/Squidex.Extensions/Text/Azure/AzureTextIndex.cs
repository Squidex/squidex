// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Models;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Contents.Text;
using Squidex.Hosting;
using Squidex.Infrastructure;

namespace Squidex.Extensions.Text.Azure
{
    public sealed class AzureTextIndex : IInitializable, ITextIndex
    {
        private readonly SearchIndexClient indexClient;
        private readonly SearchClient searchClient;

        public AzureTextIndex(
            string serviceEndpoint,
            string serviceApiKey,
            string indexName)
        {
            indexClient = new SearchIndexClient(new Uri(serviceEndpoint), new AzureKeyCredential(serviceApiKey));

            searchClient = indexClient.GetSearchClient(indexName);
        }

        public async Task InitializeAsync(
            CancellationToken ct)
        {
            await CreateIndexAsync(ct);
        }

        public async Task ClearAsync(
            CancellationToken ct = default)
        {
            await indexClient.DeleteIndexAsync(searchClient.IndexName, ct);

            await CreateIndexAsync(ct);
        }

        private async Task CreateIndexAsync(
            CancellationToken ct)
        {
            var index = AzureIndexDefinition.Create(searchClient.IndexName);

            await indexClient.CreateOrUpdateIndexAsync(index, true, true, ct);
        }

        public async Task ExecuteAsync(IndexCommand[] commands,
            CancellationToken ct = default)
        {
            var batch = IndexDocumentsBatch.Create<SearchDocument>();

            commands.Foreach(x => CommandFactory.CreateCommands(x, batch.Actions));

            await searchClient.IndexDocumentsAsync(batch, cancellationToken: ct);
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

            var searchOptions = new SearchOptions
            {
                Filter = BuildFilter(app, query, scope)
            };

            if (queryText.Length >= 4 && queryText.IndexOf(":", StringComparison.OrdinalIgnoreCase) == 2)
            {
                var candidateLanguage = queryText.Substring(0, 2);

                if (Language.IsValidLanguage(candidateLanguage))
                {
                    searchOptions.SearchFields.Add(candidateLanguage);

                    queryText = queryText[3..];
                }
            }

            searchOptions.Select.Add("contentId");
            searchOptions.Size = 2000;

            var results = await searchClient.SearchAsync<SearchDocument>(queryText, searchOptions, ct);

            var ids = new List<DomainId>();

            await foreach (var item in results.Value.GetResultsAsync().WithCancellation(ct))
            {
                if (item != null)
                {
                    ids.Add(DomainId.Create(item.Document["contentId"].ToString()));
                }
            }

            return ids;
        }

        private static string BuildFilter(IAppEntity app, TextQuery query, SearchScope scope)
        {
            var sb = new StringBuilder();

            sb.Append($"appId eq '{app.Id}' and {GetServeField(scope)} eq true");

            if (query.Filter.SchemaIds?.Length > 0 && query.Filter.Must)
            {
                var schemaIds = string.Join(",", query.Filter.SchemaIds.Select(x => $"'{x}'"));

                sb.Append($" and schemaId in ({schemaIds})");
            }

            return sb.ToString();
        }

        private static string GetServeField(SearchScope scope)
        {
            return scope == SearchScope.Published ?
                "servePublished" :
                "serveAll";
        }
    }
}
