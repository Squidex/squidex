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

namespace Squidex.Infrastructure.MongoDb.OData
{
    public sealed class FilterVisitor<T> : FilterNodeVisitor<FilterDefinition<T>>
    {
        private static readonly FilterDefinitionBuilder<T> Filter = Builders<T>.Filter;
        private static readonly FilterVisitor<T> Instance = new FilterVisitor<T>();

        private FilterVisitor()
        {
        }

        public static FilterDefinition<T> Visit(FilterNode node)
        {
            return node.Accept(Instance);
        }

        public override FilterDefinition<T> Visit(FilterNegate nodeIn)
        {
            return Filter.Not(nodeIn.Operand.Accept(this));
        }

        public override FilterDefinition<T> Visit(FilterJunction nodeIn)
        {
            if (nodeIn.JunctionType == FilterJunctionType.And)
            {
                return Filter.And(nodeIn.Operands.Select(x => x.Accept(this)));
            }
            else
            {
                return Filter.Or(nodeIn.Operands.Select(x => x.Accept(this)));
            }
        }

        public override FilterDefinition<T> Visit(FilterComparison nodeIn)
        {
            var propertyName = string.Join(".", nodeIn.Lhs);

            switch (nodeIn.Operator)
            {
                case FilterOperator.StartsWith:
                    return Filter.Regex(propertyName, BuildRegex(nodeIn, s => "$" + s));
                case FilterOperator.Contains:
                    return Filter.Regex(propertyName, BuildRegex(nodeIn, s => s));
                case FilterOperator.EndsWith:
                    return Filter.Regex(propertyName, BuildRegex(nodeIn, s => s + "$"));
                case FilterOperator.Equals:
                    return Filter.Eq(propertyName, nodeIn.Rhs.Value);
                case FilterOperator.GreaterThan:
                    return Filter.Gt(propertyName, nodeIn.Rhs.Value);
                case FilterOperator.GreaterThanOrEqual:
                    return Filter.Gte(propertyName, nodeIn.Rhs.Value);
                case FilterOperator.LessThan:
                    return Filter.Lt(propertyName, nodeIn.Rhs.Value);
                case FilterOperator.LessThanOrEqual:
                    return Filter.Lte(propertyName, nodeIn.Rhs.Value);
                case FilterOperator.NotEquals:
                    return Filter.Ne(propertyName, nodeIn.Rhs.Value);
                case FilterOperator.In:
                    return Filter.In(propertyName, ((IList)nodeIn.Rhs.Value).OfType<object>());
            }

            throw new NotSupportedException();
        }

        private static BsonRegularExpression BuildRegex(FilterComparison node, Func<string, string> formatter)
        {
            return new BsonRegularExpression(formatter(node.Rhs.Value.ToString()), "i");
        }
    }
}
