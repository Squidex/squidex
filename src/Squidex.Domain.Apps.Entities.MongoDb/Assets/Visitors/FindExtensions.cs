// ==========================================================================
//  FindExtensions.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================
using System;
using System.Collections.Generic;
using Microsoft.OData.UriParser;
using MongoDB.Bson;
using MongoDB.Driver;
using Squidex.Infrastructure.MongoDb.OData;

namespace Squidex.Domain.Apps.Entities.MongoDb.Assets.Visitors
{
    public static class FindExtensions
    {
        private static readonly FilterDefinitionBuilder<MongoAssetEntity> Filter = Builders<MongoAssetEntity>.Filter;

        public static IFindFluent<MongoAssetEntity, MongoAssetEntity> AssetSort(this IFindFluent<MongoAssetEntity, MongoAssetEntity> cursor, ODataUriParser query)
        {
            var sort = query.BuildSort<MongoAssetEntity>();

            return sort != null ? cursor.Sort(sort) : cursor.SortByDescending(x => x.LastModified);
        }

        public static IFindFluent<MongoAssetEntity, MongoAssetEntity> AssetTake(this IFindFluent<MongoAssetEntity, MongoAssetEntity> cursor, ODataUriParser query)
        {
            return cursor.Take(query, 200, 20);
        }

        public static IFindFluent<MongoAssetEntity, MongoAssetEntity> AssetSkip(this IFindFluent<MongoAssetEntity, MongoAssetEntity> cursor, ODataUriParser query)
        {
            return cursor.Skip(query);
        }

        public static FilterDefinition<MongoAssetEntity> BuildQuery(ODataUriParser query, Guid appId)
        {
            var filters = new List<FilterDefinition<MongoAssetEntity>>
            {
                Filter.Eq(x => x.AppId, appId),
                Filter.Eq(x => x.IsDeleted, false)
            };

            var filter = query.BuildFilter<MongoAssetEntity>(null, false);

            if (filter.Filter != null)
            {
                if (filter.Last)
                {
                    filters.Add(filter.Filter);
                }
                else
                {
                    filters.Insert(0, filter.Filter);
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
