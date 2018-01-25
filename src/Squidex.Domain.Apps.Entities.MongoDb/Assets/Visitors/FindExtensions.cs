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

namespace Squidex.Domain.Apps.Entities.MongoDb.Assets.Visitors
{
    public static class FindExtensions
    {
        private static readonly FilterDefinitionBuilder<MongoAssetEntity> Filter = Builders<MongoAssetEntity>.Filter;

        public static IFindFluent<MongoAssetEntity, MongoAssetEntity> Sort(this IFindFluent<MongoAssetEntity, MongoAssetEntity> cursor, ODataUriParser query)
        {
            return cursor.Sort(SortBuilder.BuildSort(query));
        }

        public static IFindFluent<MongoAssetEntity, MongoAssetEntity> Take(this IFindFluent<MongoAssetEntity, MongoAssetEntity> cursor, ODataUriParser query)
        {
            var top = query.ParseTop();

            if (top.HasValue)
            {
                cursor = cursor.Limit(Math.Min((int)top.Value, 200));
            }
            else
            {
                cursor = cursor.Limit(20);
            }

            return cursor;
        }

        public static IFindFluent<MongoAssetEntity, MongoAssetEntity> Skip(this IFindFluent<MongoAssetEntity, MongoAssetEntity> cursor, ODataUriParser query)
        {
            var skip = query.ParseSkip();

            if (skip.HasValue)
            {
                cursor = cursor.Skip((int)skip.Value);
            }
            else
            {
                cursor = cursor.Skip(null);
            }

            return cursor;
        }

        public static IFindFluent<MongoAssetEntity, MongoAssetEntity> Find(this IMongoCollection<MongoAssetEntity> cursor, ODataUriParser query, Guid appId)
        {
            var filter = BuildQuery(query, appId);

            return cursor.Find(filter);
        }

        public static FilterDefinition<MongoAssetEntity> BuildQuery(ODataUriParser query, Guid appId)
        {
            var filters = new List<FilterDefinition<MongoAssetEntity>>
            {
                Filter.Eq(x => x.AppId, appId),
                Filter.Eq(x => x.IsDeleted, false)
            };

            filters.AddRange(FilterBuilder.Build(query));

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
