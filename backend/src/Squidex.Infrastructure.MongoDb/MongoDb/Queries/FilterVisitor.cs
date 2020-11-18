// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using Squidex.Infrastructure.Queries;

namespace Squidex.Infrastructure.MongoDb.Queries
{
    public sealed class FilterVisitor<T> : FilterNodeVisitor<FilterDefinition<T>, ClrValue>
    {
        private static readonly FilterDefinitionBuilder<T> Filter = Builders<T>.Filter;
        private static readonly FilterVisitor<T> Instance = new FilterVisitor<T>();

        private FilterVisitor()
        {
        }

        public static FilterDefinition<T> Visit(FilterNode<ClrValue> node)
        {
            return node.Accept(Instance);
        }

        public override FilterDefinition<T> Visit(NegateFilter<ClrValue> nodeIn)
        {
            return Filter.Not(nodeIn.Filter.Accept(this));
        }

        public override FilterDefinition<T> Visit(LogicalFilter<ClrValue> nodeIn)
        {
            if (nodeIn.Type == LogicalFilterType.And)
            {
                return Filter.And(nodeIn.Filters.Select(x => x.Accept(this)));
            }
            else
            {
                return Filter.Or(nodeIn.Filters.Select(x => x.Accept(this)));
            }
        }

        public override FilterDefinition<T> Visit(CompareFilter<ClrValue> nodeIn)
        {
            var propertyName = nodeIn.Path.ToString();

            var value = nodeIn.Value.Value;

            switch (nodeIn.Operator)
            {
                case CompareOperator.Empty:
                    return Filter.Or(
                        Filter.Exists(propertyName, false),
                        Filter.Eq(propertyName, default(T)!),
                        Filter.Eq(propertyName, string.Empty),
                        Filter.Eq(propertyName, Array.Empty<T>()));
                case CompareOperator.StartsWith:
                    return Filter.Regex(propertyName, BuildRegex(nodeIn, s => "^" + s));
                case CompareOperator.Contains:
                    return Filter.Regex(propertyName, BuildRegex(nodeIn, s => s));
                case CompareOperator.EndsWith:
                    return Filter.Regex(propertyName, BuildRegex(nodeIn, s => s + "$"));
                case CompareOperator.Equals:
                    return Filter.Eq(propertyName, value);
                case CompareOperator.GreaterThan:
                    return Filter.Gt(propertyName, value);
                case CompareOperator.GreaterThanOrEqual:
                    return Filter.Gte(propertyName, value);
                case CompareOperator.LessThan:
                    return Filter.Lt(propertyName, value);
                case CompareOperator.LessThanOrEqual:
                    return Filter.Lte(propertyName, value);
                case CompareOperator.NotEquals:
                    return Filter.Ne(propertyName, value);
                case CompareOperator.In:
                    return Filter.In(propertyName, ((IList)value!).OfType<object>());
            }

            throw new NotSupportedException();
        }

        private static BsonRegularExpression BuildRegex(CompareFilter<ClrValue> node, Func<string, string> formatter)
        {
            return new BsonRegularExpression(formatter(node.Value.Value?.ToString() ?? "null"), "i");
        }
    }
}
