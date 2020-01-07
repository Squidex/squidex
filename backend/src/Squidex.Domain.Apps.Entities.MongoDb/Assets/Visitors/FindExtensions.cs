// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using Squidex.Infrastructure.MongoDb.Queries;
using Squidex.Infrastructure.Queries;

namespace Squidex.Domain.Apps.Entities.MongoDb.Assets.Visitors
{
    public static class FindExtensions
    {
        private static readonly FilterDefinitionBuilder<MongoAssetEntity> Filter = Builders<MongoAssetEntity>.Filter;

        public static ClrQuery AdjustToModel(this ClrQuery query)
        {
            if (query.Filter != null)
            {
                query.Filter = FirstPascalPathConverter<ClrValue>.Transform(query.Filter);
            }

            query.Sort = query.Sort
                .Select(x =>
                    new SortNode(
                        x.Path.ToFirstPascalCase(),
                        x.Order))
                    .ToList();

            return query;
        }

        public static IFindFluent<MongoAssetEntity, MongoAssetEntity> AssetSort(this IFindFluent<MongoAssetEntity, MongoAssetEntity> cursor, ClrQuery query)
        {
            return cursor.Sort(query.BuildSort<MongoAssetEntity>());
        }

        public static IFindFluent<MongoAssetEntity, MongoAssetEntity> AssetTake(this IFindFluent<MongoAssetEntity, MongoAssetEntity> cursor, ClrQuery query)
        {
            return cursor.Take(query);
        }

        public static IFindFluent<MongoAssetEntity, MongoAssetEntity> AssetSkip(this IFindFluent<MongoAssetEntity, MongoAssetEntity> cursor, ClrQuery query)
        {
            return cursor.Skip(query);
        }

        public static FilterDefinition<MongoAssetEntity> BuildFilter(this ClrQuery query, Guid appId, Guid? parentId)
        {
            var filters = new List<FilterDefinition<MongoAssetEntity>>
            {
                Filter.Eq(x => x.IndexedAppId, appId),
                Filter.Eq(x => x.IsDeleted, false)
            };

            if (parentId.HasValue)
            {
                if (parentId == Guid.Empty)
                {
                    filters.Add(
                        Filter.Or(
                            Filter.Exists(x => x.ParentId, false),
                            Filter.Eq(x => x.ParentId, Guid.Empty)));
                }
                else
                {
                    filters.Add(Filter.Eq(x => x.ParentId, parentId.Value));
                }
            }

            var (filter, last) = query.BuildFilter<MongoAssetEntity>(false);

            if (filter != null)
            {
                if (last)
                {
                    filters.Add(filter);
                }
                else
                {
                    filters.Insert(0, filter);
                }
            }

            if (filters.Count > 1)
            {
                return Filter.And(filters);
            }
            else if (filters.Count == 1)
            {
                return filters[0];
            }
            else
            {
                return new BsonDocument();
            }
        }
    }
}
