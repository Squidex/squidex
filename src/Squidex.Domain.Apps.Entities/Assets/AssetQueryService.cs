// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.OData;
using Squidex.Domain.Apps.Core.Tags;
using Squidex.Domain.Apps.Entities.Assets.Edm;
using Squidex.Domain.Apps.Entities.Assets.Queries;
using Squidex.Domain.Apps.Entities.Assets.Repositories;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Queries;
using Squidex.Infrastructure.Queries.OData;

namespace Squidex.Domain.Apps.Entities.Assets
{
    public sealed class AssetQueryService : IAssetQueryService
    {
        private readonly ITagService tagService;
        private readonly IAssetEnricher assetEnricher;
        private readonly IAssetRepository assetRepository;
        private readonly AssetOptions options;

        public int DefaultPageSizeGraphQl
        {
            get { return options.DefaultPageSizeGraphQl; }
        }

        public AssetQueryService(
            ITagService tagService,
            IAssetEnricher assetEnricher,
            IAssetRepository assetRepository,
            IOptions<AssetOptions> options)
        {
            Guard.NotNull(tagService, nameof(tagService));
            Guard.NotNull(assetEnricher, nameof(assetEnricher));
            Guard.NotNull(assetRepository, nameof(assetRepository));
            Guard.NotNull(options, nameof(options));

            this.tagService = tagService;
            this.assetEnricher = assetEnricher;
            this.assetRepository = assetRepository;
            this.options = options.Value;
        }

        public async Task<IEnrichedAssetEntity> FindAssetAsync( Guid id)
        {
            var asset = await assetRepository.FindAssetAsync(id);

            if (asset != null)
            {
                return await assetEnricher.EnrichAsync(asset);
            }

            return null;
        }

        public async Task<IReadOnlyList<IEnrichedAssetEntity>> QueryByHashAsync(Guid appId, string hash)
        {
            Guard.NotNull(hash, nameof(hash));

            var assets = await assetRepository.QueryByHashAsync(appId, hash);

            return await assetEnricher.EnrichAsync(assets);
        }

        public async Task<IResultList<IEnrichedAssetEntity>> QueryAsync(Context context, Q query)
        {
            Guard.NotNull(context, nameof(context));
            Guard.NotNull(query, nameof(query));

            IResultList<IAssetEntity> assets;

            if (query.Ids != null && query.Ids.Count > 0)
            {
                assets = await QueryByIdsAsync(context, query);
            }
            else
            {
                assets = await QueryByQueryAsync(context, query);
            }

            var enriched = await assetEnricher.EnrichAsync(assets);

            return ResultList.Create(assets.Total, enriched);
        }

        private async Task<IResultList<IAssetEntity>> QueryByQueryAsync(Context context, Q query)
        {
            var parsedQuery = ParseQuery(context, query.ODataQuery);

            return await assetRepository.QueryAsync(context.App.Id, parsedQuery);
        }

        private async Task<IResultList<IAssetEntity>> QueryByIdsAsync(Context context, Q query)
        {
            var assets = await assetRepository.QueryAsync(context.App.Id, new HashSet<Guid>(query.Ids));

            return Sort(assets, query.Ids);
        }

        private static IResultList<IAssetEntity> Sort(IResultList<IAssetEntity> assets, IReadOnlyList<Guid> ids)
        {
            return assets.SortSet(x => x.Id, ids);
        }

        private Query ParseQuery(Context context, string query)
        {
            try
            {
                var result = EdmAssetModel.Edm.ParseQuery(query).ToQuery();

                if (result.Filter != null)
                {
                    result.Filter = FilterTagTransformer.Transform(result.Filter, context.App.Id, tagService);
                }

                if (result.Sort.Count == 0)
                {
                    result.Sort.Add(new SortNode(new List<string> { "lastModified" }, SortOrder.Descending));
                }

                if (result.Take == long.MaxValue)
                {
                    result.Take = options.DefaultPageSize;
                }
                else if (result.Take > options.MaxResults)
                {
                    result.Take = options.MaxResults;
                }

                return result;
            }
            catch (NotSupportedException)
            {
                throw new ValidationException("OData operation is not supported.");
            }
            catch (ODataException ex)
            {
                throw new ValidationException($"Failed to parse query: {ex.Message}", ex);
            }
        }
    }
}
