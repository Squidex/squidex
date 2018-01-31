// ==========================================================================
//  FilterVisitor.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================
using System;
using System.Linq;
using Microsoft.OData.UriParser;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Squidex.Domain.Apps.Entities.MongoDb.Assets.Visitors
{
    public class FilterVisitor : QueryNodeVisitor<FilterDefinition<MongoAssetEntity>>
    {
        private static readonly FilterDefinitionBuilder<MongoAssetEntity> Filter = Builders<MongoAssetEntity>.Filter;

        public static FilterDefinition<MongoAssetEntity> Visit(QueryNode node)
        {
            var visitor = new FilterVisitor();

            return node.Accept(visitor);
        }

        public override FilterDefinition<MongoAssetEntity> Visit(ConvertNode nodeIn)
        {
            return nodeIn.Source.Accept(this);
        }

        public override FilterDefinition<MongoAssetEntity> Visit(UnaryOperatorNode nodeIn)
        {
            if (nodeIn.OperatorKind == UnaryOperatorKind.Not)
            {
                return Filter.Not(nodeIn.Operand.Accept(this));
            }

            throw new NotSupportedException();
        }

        public override FilterDefinition<MongoAssetEntity> Visit(SingleValueFunctionCallNode nodeIn)
        {
            var fieldNode = nodeIn.Parameters.ElementAt(0);
            var valueNode = nodeIn.Parameters.ElementAt(1);

            if (string.Equals(nodeIn.Name, "endswith", StringComparison.OrdinalIgnoreCase))
            {
                var value = BuildRegex(valueNode, v => v + "$");

                return Filter.Regex(BuildFieldDefinition(fieldNode), value);
            }

            if (string.Equals(nodeIn.Name, "startswith", StringComparison.OrdinalIgnoreCase))
            {
                var value = BuildRegex(valueNode, v => "^" + v);

                return Filter.Regex(BuildFieldDefinition(fieldNode), value);
            }

            if (string.Equals(nodeIn.Name, "contains", StringComparison.OrdinalIgnoreCase))
            {
                var value = BuildRegex(valueNode, v => v);

                return Filter.Regex(BuildFieldDefinition(fieldNode), value);
            }

            throw new NotSupportedException();
        }

        public override FilterDefinition<MongoAssetEntity> Visit(BinaryOperatorNode nodeIn)
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
                    var field = BuildFieldDefinition(nodeIn.Left);

                    return Filter.Or(
                        Filter.Not(Filter.Exists(field)),
                        Filter.Ne(field, BuildValue(nodeIn.Right)));
                }

                if (nodeIn.OperatorKind == BinaryOperatorKind.Equal)
                {
                    return Filter.Eq(BuildFieldDefinition(nodeIn.Left), BuildValue(nodeIn.Right));
                }

                if (nodeIn.OperatorKind == BinaryOperatorKind.LessThan)
                {
                    return Filter.Lt(BuildFieldDefinition(nodeIn.Left), BuildValue(nodeIn.Right));
                }

                if (nodeIn.OperatorKind == BinaryOperatorKind.LessThanOrEqual)
                {
                    return Filter.Lte(BuildFieldDefinition(nodeIn.Left), BuildValue(nodeIn.Right));
                }

                if (nodeIn.OperatorKind == BinaryOperatorKind.GreaterThan)
                {
                    return Filter.Gt(BuildFieldDefinition(nodeIn.Left), BuildValue(nodeIn.Right));
                }

                if (nodeIn.OperatorKind == BinaryOperatorKind.GreaterThanOrEqual)
                {
                    return Filter.Gte(BuildFieldDefinition(nodeIn.Left), BuildValue(nodeIn.Right));
                }
            }

            throw new NotSupportedException();
        }

        private static BsonRegularExpression BuildRegex(QueryNode node, Func<string, string> formatter)
        {
            return new BsonRegularExpression(formatter(BuildValue(node).ToString()), "i");
        }

        private FieldDefinition<MongoAssetEntity, object> BuildFieldDefinition(QueryNode nodeIn)
        {
            return PropertyVisitor.Visit(nodeIn);
        }

        private static object BuildValue(QueryNode nodeIn)
        {
            return ConstantVisitor.Visit(nodeIn);
        }
    }
}
