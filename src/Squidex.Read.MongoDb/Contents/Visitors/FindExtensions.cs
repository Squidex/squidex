// ==========================================================================
//  FindExtensions.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using Microsoft.OData.Core.UriParser;
using MongoDB.Bson;
using MongoDB.Driver;
using Squidex.Core.Schemas;

// ReSharper disable ConvertIfStatementToConditionalTernaryExpression
// ReSharper disable RedundantIfElseBlock

namespace Squidex.Read.MongoDb.Contents.Visitors
{
    public static class FindExtensions
    {
        private static readonly FilterDefinitionBuilder<MongoContentEntity> Filter = Builders<MongoContentEntity>.Filter;

        public static IFindFluent<MongoContentEntity, MongoContentEntity> Sort(this IFindFluent<MongoContentEntity, MongoContentEntity>  cursor, ODataUriParser query, Schema schema)
        {
            return cursor.Sort(SortBuilder.BuildSort(query, schema));
        }

        public static IFindFluent<MongoContentEntity, MongoContentEntity> Take(this IFindFluent<MongoContentEntity, MongoContentEntity> cursor, ODataUriParser query)
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

        public static IFindFluent<MongoContentEntity, MongoContentEntity> Skip(this IFindFluent<MongoContentEntity, MongoContentEntity> cursor, ODataUriParser query)
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

        public static IFindFluent<MongoContentEntity, MongoContentEntity> Find(this IMongoCollection<MongoContentEntity> cursor, ODataUriParser query, Guid schemaId, Schema schema, bool nonPublished)
        {
            var filter = BuildQuery(query, schemaId, schema, nonPublished);

            return cursor.Find(filter);
        }

        public static FilterDefinition<MongoContentEntity> BuildQuery(ODataUriParser query, Guid schemaId, Schema schema, bool nonPublished)
        {
            var filters = new List<FilterDefinition<MongoContentEntity>>
            {
                Filter.Eq(x => x.SchemaId, schemaId)
            };

            if (!nonPublished)
            {
                filters.Add(Filter.Eq(x => x.IsPublished, true));
            }

            var filter = FilterBuilder.Build(query, schema);

            if (filter != null)
            {
                filters.Add(filter);
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
