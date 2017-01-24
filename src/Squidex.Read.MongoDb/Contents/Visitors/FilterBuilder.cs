// ==========================================================================
//  FilterBuilder.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Microsoft.OData.Core.UriParser;
using MongoDB.Driver;
using Squidex.Core.Schemas;
// ReSharper disable ConvertIfStatementToReturnStatement

namespace Squidex.Read.MongoDb.Contents.Visitors
{
    public static class FilterBuilder
    {
        private static readonly FilterDefinitionBuilder<MongoContentEntity> Filter = Builders<MongoContentEntity>.Filter;

        public static FilterDefinition<MongoContentEntity> Build(ODataUriParser query, Schema schema)
        {
            var search = query.ParseSearch();

            if (search != null)
            {
                return Filter.Text(ConstantVisitor.Visit(search.Expression).ToString());
            }

            var filter = query.ParseFilter();

            if (filter != null)
            {
                return FilterVisitor.Visit(filter.Expression, schema);
            }

            return null;
        }
    }
}
