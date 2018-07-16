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
using Microsoft.OData.UriParser;
using MongoDB.Bson;
using MongoDB.Driver;
using Squidex.Domain.Apps.Entities.Tags;
using Squidex.Infrastructure;
using Squidex.Infrastructure.MongoDb.OData;

namespace Squidex.Domain.Apps.Entities.MongoDb.Assets.Visitors
{
    public static class FindExtensions
    {
        private static readonly FilterDefinitionBuilder<MongoAssetEntity> Filter = Builders<MongoAssetEntity>.Filter;
        private static readonly ConvertProperty PropertyCalculator = propertyNames =>
        {
            if (propertyNames.Length > 0)
            {
                propertyNames[0] = propertyNames[0].ToPascalCase();
            }

            var propertyName = string.Join(".", propertyNames);

            return propertyName;
        };

        public static IFindFluent<MongoAssetEntity, MongoAssetEntity> AssetSort(this IFindFluent<MongoAssetEntity, MongoAssetEntity> cursor, ODataUriParser query)
        {
            var sort = query.BuildSort<MongoAssetEntity>(PropertyCalculator);

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

        public static FilterDefinition<MongoAssetEntity> BuildQuery(ODataUriParser query, Guid appId, ITagService tagService)
        {
            var convertValue = CreateValueConverter(appId, tagService);

            var filters = new List<FilterDefinition<MongoAssetEntity>>
            {
                Filter.Eq(x => x.IndexedAppId, appId),
                Filter.Eq(x => x.IsDeleted, false)
            };

            var filter = query.BuildFilter<MongoAssetEntity>(PropertyCalculator, convertValue, false);

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

        public static ConvertValue CreateValueConverter(Guid appId, ITagService tagService)
        {
            return new ConvertValue((field, value) =>
            {
                if (string.Equals(field, nameof(MongoAssetEntity.Tags), StringComparison.OrdinalIgnoreCase))
                {
                    var tagNames = Task.Run(() => tagService.GetTagIdsAsync(appId, TagGroups.Assets, HashSet.Of(value.ToString()))).Result;

                    return tagNames?.FirstOrDefault() ?? value;
                }

                return value;
            });
        }
    }
}
