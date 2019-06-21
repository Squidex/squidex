// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.DataLoader;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Log;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL
{
    public sealed class GraphQLExecutionContext : QueryExecutionContext
    {
        private static readonly List<IAssetEntity> EmptyAssets = new List<IAssetEntity>();
        private static readonly List<IContentEntity> EmptyContents = new List<IContentEntity>();
        private readonly IDataLoaderContextAccessor dataLoaderContextAccessor;
        private readonly IDependencyResolver resolver;

        public IGraphQLUrlGenerator UrlGenerator { get; }

        public ISemanticLog Log { get; }

        public GraphQLExecutionContext(QueryContext context, IDependencyResolver resolver)
            : base(context,
                resolver.Resolve<IAssetQueryService>(),
                resolver.Resolve<IContentQueryService>())
        {
            UrlGenerator = resolver.Resolve<IGraphQLUrlGenerator>();

            dataLoaderContextAccessor = resolver.Resolve<IDataLoaderContextAccessor>();

            this.resolver = resolver;
        }

        public void Setup(ExecutionOptions execution)
        {
            var loader = resolver.Resolve<DataLoaderDocumentListener>();

            var logger = LoggingMiddleware.Create(resolver.Resolve<ISemanticLog>());

            execution.Listeners.Add(loader);
            execution.FieldMiddleware.Use(logger);

            execution.UserContext = this;
        }

        public async Task<IReadOnlyList<IAssetEntity>> GetReferencedAssetsAsync(IJsonValue value)
        {
            var ids = ParseIds(value);

            if (ids == null)
            {
                return EmptyAssets;
            }

            var dataLoader =
                dataLoaderContextAccessor.Context.GetOrAddBatchLoader<Guid, IAssetEntity>("Assets",
                    async batch =>
                    {
                        var result = await GetReferencedAssetsAsync(new List<Guid>(batch));

                        return result.ToDictionary(x => x.Id);
                    });

            var assets = await Task.WhenAll(ids.Select(x => dataLoader.LoadAsync(x)));

            return assets.Where(x => x != null).ToList();
        }

        public async Task<IReadOnlyList<IContentEntity>> GetReferencedContentsAsync(Guid schemaId, IJsonValue value)
        {
            var ids = ParseIds(value);

            if (ids == null)
            {
                return EmptyContents;
            }

            var dataLoader =
                dataLoaderContextAccessor.Context.GetOrAddBatchLoader<Guid, IContentEntity>($"Schema_{schemaId}",
                    async batch =>
                    {
                        var result = await GetReferencedContentsAsync(schemaId, new List<Guid>(batch));

                        return result.ToDictionary(x => x.Id);
                    });

            var contents = await Task.WhenAll(ids.Select(x => dataLoader.LoadAsync(x)));

            return contents.Where(x => x != null).ToList();
        }

        private static ICollection<Guid> ParseIds(IJsonValue value)
        {
            try
            {
                var result = new List<Guid>();

                if (value is JsonArray array)
                {
                    foreach (var id in array)
                    {
                        result.Add(Guid.Parse(id.ToString()));
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
