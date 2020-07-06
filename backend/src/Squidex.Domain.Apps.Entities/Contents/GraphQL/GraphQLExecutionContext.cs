// ==========================================================================
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
using Squidex.Domain.Apps.Entities.Contents.GraphQL.Types;
using Squidex.Domain.Apps.Entities.Contents.Queries;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Log;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL
{
    public sealed class GraphQLExecutionContext : QueryExecutionContext
    {
        private static readonly IReadOnlyList<IEnrichedAssetEntity> EmptyAssets = new List<IEnrichedAssetEntity>();
        private static readonly IReadOnlyList<IContentEntity> EmptyContents = new List<IContentEntity>();
        private readonly IDataLoaderContextAccessor dataLoaderContextAccessor;
        private readonly IDependencyResolver resolver;

        public IUrlGenerator UrlGenerator { get; }

        public ISemanticLog Log { get; }

        public GraphQLExecutionContext(Context context, IDependencyResolver resolver)
            : base(context
                    .WithoutCleanup()
                    .WithoutContentEnrichment(),
                resolver.Resolve<IAssetQueryService>(),
                resolver.Resolve<IContentQueryService>())
        {
            UrlGenerator = resolver.Resolve<IUrlGenerator>();

            dataLoaderContextAccessor = resolver.Resolve<IDataLoaderContextAccessor>();

            this.resolver = resolver;
        }

        public void Setup(ExecutionOptions execution)
        {
            var loader = resolver.Resolve<DataLoaderDocumentListener>();

            execution.Listeners.Add(loader);
            execution.FieldMiddleware.Use(Middlewares.Logging(resolver.Resolve<ISemanticLog>()));
            execution.FieldMiddleware.Use(Middlewares.Errors());

            execution.UserContext = this;
        }

        public override async Task<IEnrichedAssetEntity?> FindAssetAsync(DomainId id)
        {
            var dataLoader = GetAssetsLoader();

            return await dataLoader.LoadAsync(id);
        }

        public async Task<IContentEntity?> FindContentAsync(DomainId id)
        {
            var dataLoader = GetContentsLoader();

            return await dataLoader.LoadAsync(id);
        }

        public async Task<IReadOnlyList<IEnrichedAssetEntity>> GetReferencedAssetsAsync(IJsonValue value)
        {
            var ids = ParseIds(value);

            if (ids == null)
            {
                return EmptyAssets;
            }

            var dataLoader = GetAssetsLoader();

            return await dataLoader.LoadManyAsync(ids);
        }

        public async Task<IReadOnlyList<IContentEntity>> GetReferencedContentsAsync(IJsonValue value)
        {
            var ids = ParseIds(value);

            if (ids == null)
            {
                return EmptyContents;
            }

            var dataLoader = GetContentsLoader();

            return await dataLoader.LoadManyAsync(ids);
        }

        private IDataLoader<DomainId, IEnrichedAssetEntity> GetAssetsLoader()
        {
            return dataLoaderContextAccessor.Context.GetOrAddBatchLoader<DomainId, IEnrichedAssetEntity>("Assets",
                async batch =>
                {
                    var result = await GetReferencedAssetsAsync(new List<DomainId>(batch));

                    return result.ToDictionary(x => x.Id);
                });
        }

        private IDataLoader<DomainId, IContentEntity> GetContentsLoader()
        {
            return dataLoaderContextAccessor.Context.GetOrAddBatchLoader<DomainId, IContentEntity>("References",
                async batch =>
                {
                    var result = await GetReferencedContentsAsync(new List<DomainId>(batch));

                    return result.ToDictionary(x => x.Id);
                });
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
                        result.Add(id.ToString());
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
