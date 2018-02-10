// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.OData.UriParser;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.GenerateEdmSchema;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure.MongoDb.OData;

namespace Squidex.Domain.Apps.Entities.MongoDb.Contents.Visitors
{
    public static class FindExtensions
    {
        private static readonly FilterDefinitionBuilder<MongoContentEntity> Filter = Builders<MongoContentEntity>.Filter;

        private static readonly Dictionary<string, string> PropertyMap =
            typeof(MongoContentEntity).GetProperties()
                .ToDictionary(x => x.Name, x => x.GetCustomAttribute<BsonElementAttribute>()?.ElementName ?? x.Name, StringComparer.OrdinalIgnoreCase);

        static FindExtensions()
        {
            PropertyMap["Data"] = "do";
        }

        public static PropertyCalculator CreatePropertyCalculator(Schema schema)
        {
            return propertyNames =>
            {
                if (propertyNames.Length > 1)
                {
                    var edmName = propertyNames[1].UnescapeEdmField();

                    if (!schema.FieldsByName.TryGetValue(edmName, out var field))
                    {
                        throw new NotSupportedException();
                    }

                    propertyNames[1] = field.Id.ToString();
                }

                if (propertyNames.Length > 0)
                {
                    propertyNames[0] = PropertyMap[propertyNames[0]];
                }

                var propertyName = string.Join(".", propertyNames);

                return propertyName;
            };
        }

        public static IFindFluent<MongoContentEntity, MongoContentEntity> ContentSort(this IFindFluent<MongoContentEntity, MongoContentEntity> cursor, ODataUriParser query, PropertyCalculator propertyCalculator)
        {
            var sort = query.BuildSort<MongoContentEntity>(propertyCalculator);

            return sort != null ? cursor.Sort(sort) : cursor.SortByDescending(x => x.LastModified);
        }

        public static IFindFluent<MongoContentEntity, MongoContentEntity> ContentTake(this IFindFluent<MongoContentEntity, MongoContentEntity> cursor, ODataUriParser query)
        {
            return cursor.Take(query, 200, 20);
        }

        public static IFindFluent<MongoContentEntity, MongoContentEntity> ContentSkip(this IFindFluent<MongoContentEntity, MongoContentEntity> cursor, ODataUriParser query)
        {
            return cursor.Skip(query);
        }

        public static FilterDefinition<MongoContentEntity> BuildQuery(ODataUriParser query, Guid schemaId, Status[] status, PropertyCalculator propertyCalculator)
        {
            var filters = new List<FilterDefinition<MongoContentEntity>>
            {
                Filter.Eq(x => x.SchemaIdId, schemaId),
                Filter.In(x => x.Status, status),
                Filter.Eq(x => x.IsDeleted, false)
            };

            var filter = query.BuildFilter<MongoContentEntity>(propertyCalculator);

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

            if (filters.Count == 1)
            {
                return filters[0];
            }
            else
            {
                return Filter.And(filters);
            }
        }
    }
}
