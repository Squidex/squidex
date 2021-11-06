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
        private readonly int waitAfterUpdate;

        public AzureTextIndex(
            string serviceEndpoint,
            string serviceApiKey,
            string indexName,
            int waitAfterUpdate = 0)
        {
            indexClient = new SearchIndexClient(new Uri(serviceEndpoint), new AzureKeyCredential(serviceApiKey));

            searchClient = indexClient.GetSearchClient(indexName);

            this.waitAfterUpdate = waitAfterUpdate;
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

            if (batch.Actions.Count == 0)
            {
                return;
            }

            await searchClient.IndexDocumentsAsync(batch, cancellationToken: ct);

            if (waitAfterUpdate > 0)
            {
                await Task.Delay(waitAfterUpdate, ct);
            }
        }

        public Task<List<DomainId>> SearchAsync(IAppEntity app, GeoQuery query, SearchScope scope,
            CancellationToken ct = default)
        {
            Guard.NotNull(app, nameof(app));
            Guard.NotNull(query, nameof(query));

            return Task.FromResult<List<DomainId>>(null);
        }

        public async Task<List<DomainId>> SearchAsync(IAppEntity app, TextQuery query, SearchScope scope,
            CancellationToken ct = default)
        {
            Guard.NotNull(app, nameof(app));
            Guard.NotNull(query, nameof(query));

            if (string.IsNullOrWhiteSpace(query.Text))
            {
                return null;
            }

            List<(DomainId, double)> documents;

            if (query.RequiredSchemaIds?.Count > 0)
            {
                documents = await SearchBySchemaAsync(query.Text, query.RequiredSchemaIds, scope, query.Take, 1, ct);
            }
            else if (query.PreferredSchemaId == null)
            {
                documents = await SearchByAppAsync(query.Text, app, scope, query.Take, 1, ct);
            }
            else
            {
                var halfBucket = query.Take / 2;

                var schemaIds = Enumerable.Repeat(query.PreferredSchemaId.Value, 1);

                documents = await SearchBySchemaAsync(
                    query.Text,
                    schemaIds,
                    scope,
                    halfBucket, 1,
                    ct);

                documents.AddRange(await SearchByAppAsync(query.Text, app, scope, halfBucket, 1, ct));
            }

            return documents.OrderByDescending(x => x.Item2).Select(x => x.Item1).Distinct().ToList();
        }

        private Task<List<(DomainId, double)>> SearchBySchemaAsync(string search, IEnumerable<DomainId> schemaIds, SearchScope scope, int limit, double factor,
            CancellationToken ct = default)
        {
            var filter = $"{string.Join(" or ", schemaIds.Select(x => $"schemaId eq '{x}'"))} and {GetServeField(scope)} eq true";

            return SearchAsync(search, filter, limit, factor, ct);
        }

        private Task<List<(DomainId, double)>> SearchByAppAsync(string search, IAppEntity app, SearchScope scope, int limit, double factor,
            CancellationToken ct = default)
        {
            var filter = $"appId eq '{app.Id}' and {GetServeField(scope)} eq true";

            return SearchAsync(search, filter, limit, factor, ct);
        }

        private async Task<List<(DomainId, double)>> SearchAsync(string search, string filter, int size, double factor,
            CancellationToken ct = default)
        {
            var searchOptions = new SearchOptions
            {
                Filter = filter
            };

            searchOptions.Select.Add("contentId");
            searchOptions.Size = size;
            searchOptions.QueryType = SearchQueryType.Full;

            var results = await searchClient.SearchAsync<SearchDocument>(search, searchOptions, ct);

            var ids = new List<(DomainId, double)>();

            await foreach (var item in results.Value.GetResultsAsync().WithCancellation(ct))
            {
                if (item != null)
                {
                    ids.Add((DomainId.Create(item.Document["contentId"].ToString()), factor * item.Score ?? 0));
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
