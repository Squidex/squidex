// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using Microsoft.OData.UriParser;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Squidex.Infrastructure.MongoDb.OData
{
    public sealed class FilterVisitor<T> : QueryNodeVisitor<FilterDefinition<T>>
    {
        private static readonly FilterDefinitionBuilder<T> Filter = Builders<T>.Filter;
        private readonly ConvertProperty convertProperty;
        private readonly ConvertValue convertValue;

        private FilterVisitor(ConvertProperty convertProperty, ConvertValue convertValue)
        {
            this.convertProperty = convertProperty;
            this.convertValue = convertValue;
        }

        public static FilterDefinition<T> Visit(QueryNode node, ConvertProperty propertyCalculator, ConvertValue convertValue)
        {
            var visitor = new FilterVisitor<T>(propertyCalculator, convertValue);

            return node.Accept(visitor);
        }

        public override FilterDefinition<T> Visit(ConvertNode nodeIn)
        {
            return nodeIn.Source.Accept(this);
        }

        public override FilterDefinition<T> Visit(UnaryOperatorNode nodeIn)
        {
            if (nodeIn.OperatorKind == UnaryOperatorKind.Not)
            {
                return Filter.Not(nodeIn.Operand.Accept(this));
            }

            throw new NotSupportedException();
        }

        public override FilterDefinition<T> Visit(SingleValueFunctionCallNode nodeIn)
        {
            var fieldNode = nodeIn.Parameters.ElementAt(0);
            var valueNode = nodeIn.Parameters.ElementAt(1);

            if (string.Equals(nodeIn.Name, "endswith", StringComparison.OrdinalIgnoreCase))
            {
                var f = BuildFieldDefinition(fieldNode);
                var v = BuildRegex(f, valueNode, s => s + "$");

                return Filter.Regex(f, v);
            }

            if (string.Equals(nodeIn.Name, "startswith", StringComparison.OrdinalIgnoreCase))
            {
                var f = BuildFieldDefinition(fieldNode);
                var v = BuildRegex(f, valueNode, s => "^" + s);

                return Filter.Regex(f, v);
            }

            if (string.Equals(nodeIn.Name, "contains", StringComparison.OrdinalIgnoreCase))
            {
                var f = BuildFieldDefinition(fieldNode);
                var v = BuildRegex(f, valueNode, s => s);

                return Filter.Regex(f, v);
            }

            throw new NotSupportedException();
        }

        public override FilterDefinition<T> Visit(BinaryOperatorNode nodeIn)
        {
            if (nodeIn.OperatorKind == BinaryOperatorKind.And)
            {
                return Filter.And(nodeIn.Left.Accept(this), nodeIn.Right.Accept(this));
            }

            if (nodeIn.OperatorKind == BinaryOperatorKind.Or)
            {
                return Filter.Or(nodeIn.Left.Accept(this), nodeIn.Right.Accept(this));
            }

            if (nodeIn.Left is SingleValueFunctionCallNode functionNode)
            {
                var regexFilter = Visit(functionNode);

                var value = BuildValue(nodeIn.Right);

                if (value is bool booleanRight)
                {
                    if ((nodeIn.OperatorKind == BinaryOperatorKind.Equal && !booleanRight) ||
                        (nodeIn.OperatorKind == BinaryOperatorKind.NotEqual && booleanRight))
                    {
                        regexFilter = Filter.Not(regexFilter);
                    }

                    return regexFilter;
                }
            }
            else
            {
                if (nodeIn.OperatorKind == BinaryOperatorKind.NotEqual)
                {
                    var f = BuildFieldDefinition(nodeIn.Left);
                    var v = BuildValue(f, nodeIn.Right);

                    return Filter.Or(Filter.Not(Filter.Exists(f)), Filter.Ne(f, v));
                }

                if (nodeIn.OperatorKind == BinaryOperatorKind.Equal)
                {
                    var f = BuildFieldDefinition(nodeIn.Left);
                    var v = BuildValue(f, nodeIn.Right);

                    return Filter.Eq(f, v);
                }

                if (nodeIn.OperatorKind == BinaryOperatorKind.LessThan)
                {
                    var f = BuildFieldDefinition(nodeIn.Left);
                    var v = BuildValue(f, nodeIn.Right);

                    return Filter.Lt(f, v);
                }

                if (nodeIn.OperatorKind == BinaryOperatorKind.LessThanOrEqual)
                {
                    var f = BuildFieldDefinition(nodeIn.Left);
                    var v = BuildValue(f, nodeIn.Right);

                    return Filter.Lte(f, v);
                }

                if (nodeIn.OperatorKind == BinaryOperatorKind.GreaterThan)
                {
                    var f = BuildFieldDefinition(nodeIn.Left);
                    var v = BuildValue(f, nodeIn.Right);

                    return Filter.Gt(f, v);
                }

                if (nodeIn.OperatorKind == BinaryOperatorKind.GreaterThanOrEqual)
                {
                    var f = BuildFieldDefinition(nodeIn.Left);
                    var v = BuildValue(f, nodeIn.Right);

                    return Filter.Gte(f, v);
                }
            }

            throw new NotSupportedException();
        }

        private BsonRegularExpression BuildRegex(string field, QueryNode node, Func<string, string> formatter)
        {
            return new BsonRegularExpression(formatter(BuildValue(field, node).ToString()), "i");
        }

        private string BuildFieldDefinition(QueryNode nodeIn)
        {
            return nodeIn.BuildFieldDefinition(convertProperty);
        }

        private object BuildValue(string field, QueryNode nodeIn)
        {
            return ValueConversion.Convert(field, ConstantVisitor.Visit(nodeIn), convertValue);
        }

        private object BuildValue(QueryNode nodeIn)
        {
            return ConstantVisitor.Visit(nodeIn);
        }
    }
}
