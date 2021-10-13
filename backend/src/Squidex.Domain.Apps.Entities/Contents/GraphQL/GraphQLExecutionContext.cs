// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GraphQL.DataLoader;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Contents.Queries;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Log;
using Squidex.Shared.Users;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL
{
    public sealed class GraphQLExecutionContext : QueryExecutionContext
    {
        private static readonly List<IEnrichedAssetEntity> EmptyAssets = new List<IEnrichedAssetEntity>();
        private static readonly List<IEnrichedContentEntity> EmptyContents = new List<IEnrichedContentEntity>();
        private readonly IDataLoaderContextAccessor dataLoaders;
        private readonly IUserResolver userResolver;

        public IUrlGenerator UrlGenerator { get; }

        public ICommandBus CommandBus { get; }

        public ISemanticLog Log { get; }

        public override Context Context { get; }

        public GraphQLExecutionContext(IAssetQueryService assetQuery, IContentQueryService contentQuery,
            Context context,
            IDataLoaderContextAccessor dataLoaders,
            ICommandBus commandBus, IUrlGenerator urlGenerator, IUserResolver userResolver,
            ISemanticLog log)
            : base(assetQuery, contentQuery)
        {
            this.dataLoaders = dataLoaders;
            this.userResolver = userResolver;

            CommandBus = commandBus;

            UrlGenerator = urlGenerator;

            Context = context.Clone(b => b
                .WithoutCleanup()
                .WithoutContentEnrichment());

            Log = log;
        }

        public async Task<IUser> FindUserAsync(RefToken refToken,
            CancellationToken ct)
        {
            if (refToken.IsClient)
            {
                return new ClientUser(refToken);
            }
            else
            {
                var dataLoader = GetUserLoader();

                return await dataLoader.LoadAsync(refToken.Identifier).GetResultAsync(ct);
            }
        }

        public async Task<IEnrichedAssetEntity?> FindAssetAsync(DomainId id,
            CancellationToken ct)
        {
            var dataLoader = GetAssetsLoader();

            return await dataLoader.LoadAsync(id).GetResultAsync(ct);
        }

        public async Task<IContentEntity?> FindContentAsync(DomainId schemaId, DomainId id,
            CancellationToken ct)
        {
            var dataLoader = GetContentsLoader();

            var content = await dataLoader.LoadAsync(id).GetResultAsync(ct);

            if (content?.SchemaId.Id != schemaId)
            {
                content = null;
            }

            return content;
        }

        public Task<IReadOnlyList<IEnrichedAssetEntity>> GetReferencedAssetsAsync(IJsonValue value,
            CancellationToken ct)
        {
            var ids = ParseIds(value);

            if (ids == null)
            {
                return Task.FromResult<IReadOnlyList<IEnrichedAssetEntity>>(EmptyAssets);
            }

            var dataLoader = GetAssetsLoader();

            return LoadManyAsync(dataLoader, ids, ct);
        }

        public Task<IReadOnlyList<IEnrichedContentEntity>> GetReferencedContentsAsync(IJsonValue value,
            CancellationToken ct)
        {
            var ids = ParseIds(value);

            if (ids == null)
            {
                return Task.FromResult<IReadOnlyList<IEnrichedContentEntity>>(EmptyContents);
            }

            var dataLoader = GetContentsLoader();

            return LoadManyAsync(dataLoader, ids, ct);
        }

        private IDataLoader<DomainId, IEnrichedAssetEntity> GetAssetsLoader()
        {
            return dataLoaders.Context.GetOrAddBatchLoader<DomainId, IEnrichedAssetEntity>(nameof(GetAssetsLoader),
                async (batch, ct) =>
                {
                    var result = await GetReferencedAssetsAsync(new List<DomainId>(batch), ct);

                    return result.ToDictionary(x => x.Id);
                });
        }

        private IDataLoader<DomainId, IEnrichedContentEntity> GetContentsLoader()
        {
            return dataLoaders.Context.GetOrAddBatchLoader<DomainId, IEnrichedContentEntity>(nameof(GetContentsLoader),
                async (batch, ct) =>
                {
                    var result = await GetReferencedContentsAsync(new List<DomainId>(batch), ct);

                    return result.ToDictionary(x => x.Id);
                });
        }

        private IDataLoader<string, IUser> GetUserLoader()
        {
            return dataLoaders.Context.GetOrAddBatchLoader<string, IUser>(nameof(GetUserLoader),
                async (batch, ct) =>
                {
                    var result = await userResolver.QueryManyAsync(batch.ToArray(), ct);

                    return result;
                });
        }

        private static async Task<IReadOnlyList<T>> LoadManyAsync<TKey, T>(IDataLoader<TKey, T> dataLoader, ICollection<TKey> keys,
            CancellationToken ct) where T : class
        {
            var contents = await Task.WhenAll(keys.Select(x => dataLoader.LoadAsync(x).GetResultAsync(ct)));

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
