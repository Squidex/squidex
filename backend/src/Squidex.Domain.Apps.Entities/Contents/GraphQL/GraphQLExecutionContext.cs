﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.DataLoader;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Contents.Queries;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Log;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL
{
    public sealed class GraphQLExecutionContext : QueryExecutionContext
    {
        private static readonly List<IEnrichedAssetEntity> EmptyAssets = new List<IEnrichedAssetEntity>();
        private static readonly List<IEnrichedContentEntity> EmptyContents = new List<IEnrichedContentEntity>();
        private readonly IDataLoaderContextAccessor dataLoaderContextAccessor;
        private readonly DataLoaderDocumentListener dataLoaderDocumentListener;
        private readonly IUrlGenerator urlGenerator;
        private readonly ISemanticLog log;
        private readonly ICommandBus commandBus;
        private Context context;

        public IUrlGenerator UrlGenerator
        {
            get { return urlGenerator; }
        }

        public ICommandBus CommandBus
        {
            get { return commandBus; }
        }

        public ISemanticLog Log
        {
            get { return log; }
        }

        public override Context Context
        {
            get { return context; }
        }

        public GraphQLExecutionContext(IAssetQueryService assetQuery, IContentQueryService contentQuery,
            IDataLoaderContextAccessor dataLoaderContextAccessor, DataLoaderDocumentListener dataLoaderDocumentListener, ICommandBus commandBus, IUrlGenerator urlGenerator, ISemanticLog log)
            : base(assetQuery, contentQuery)
        {
            this.commandBus = commandBus;
            this.dataLoaderContextAccessor = dataLoaderContextAccessor;
            this.dataLoaderDocumentListener = dataLoaderDocumentListener;
            this.urlGenerator = urlGenerator;
            this.log = log;
        }

        public GraphQLExecutionContext WithContext(Context newContext)
        {
            context = newContext
                .Clone(b => b
                    .WithoutCleanup()
                    .WithoutContentEnrichment());

            return this;
        }

        public void Setup(ExecutionOptions execution)
        {
            execution.Listeners.Add(dataLoaderDocumentListener);
            execution.UserContext = this;
        }

        public async Task<IEnrichedAssetEntity?> FindAssetAsync(DomainId id)
        {
            var dataLoader = GetAssetsLoader();

            return await dataLoader.LoadAsync(id).GetResultAsync();
        }

        public async Task<IContentEntity?> FindContentAsync(DomainId id)
        {
            var dataLoader = GetContentsLoader();

            return await dataLoader.LoadAsync(id).GetResultAsync();
        }

        public Task<IReadOnlyList<IEnrichedAssetEntity>> GetReferencedAssetsAsync(IJsonValue value)
        {
            var ids = ParseIds(value);

            if (ids == null)
            {
                return Task.FromResult<IReadOnlyList<IEnrichedAssetEntity>>(EmptyAssets);
            }

            var dataLoader = GetAssetsLoader();

            return LoadManyAsync(dataLoader, ids);
        }

        public Task<IReadOnlyList<IEnrichedContentEntity>> GetReferencedContentsAsync(IJsonValue value)
        {
            var ids = ParseIds(value);

            if (ids == null)
            {
                return Task.FromResult<IReadOnlyList<IEnrichedContentEntity>>(EmptyContents);
            }

            var dataLoader = GetContentsLoader();

            return LoadManyAsync(dataLoader, ids);
        }

        private IDataLoader<DomainId, IEnrichedAssetEntity> GetAssetsLoader()
        {
            return dataLoaderContextAccessor.Context.GetOrAddBatchLoader<DomainId, IEnrichedAssetEntity>(nameof(GetAssetsLoader),
                async batch =>
                {
                    var result = await GetReferencedAssetsAsync(new List<DomainId>(batch));

                    return result.ToDictionary(x => x.Id);
                });
        }

        private IDataLoader<DomainId, IEnrichedContentEntity> GetContentsLoader()
        {
            return dataLoaderContextAccessor.Context.GetOrAddBatchLoader<DomainId, IEnrichedContentEntity>(nameof(GetContentsLoader),
                async batch =>
                {
                    var result = await GetReferencedContentsAsync(new List<DomainId>(batch));

                    return result.ToDictionary(x => x.Id);
                });
        }

        private static async Task<IReadOnlyList<T>> LoadManyAsync<TKey, T>(IDataLoader<TKey, T> dataLoader, ICollection<TKey> keys) where T : class
        {
            var contents = await Task.WhenAll(keys.Select(x => dataLoader.LoadAsync(x).GetResultAsync()));

            return contents.NotNull().ToList();
        }

        private static ICollection<DomainId>? ParseIds(IJsonValue value)
        {
            try
            {
                var result = new List<DomainId>();

                if (value is JsonArray array)
                {
                    foreach (var id in array)
                    {
                        result.Add(DomainId.Create(id.ToString()));
                    }
                }

                return result;
            }
            catch
            {
                return null;
            }
        }
    }
}
