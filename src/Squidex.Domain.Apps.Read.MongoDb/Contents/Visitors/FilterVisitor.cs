// ==========================================================================
//  FilterVisitor.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Linq;
using Microsoft.OData.Core.UriParser.Semantic;
using Microsoft.OData.Core.UriParser.TreeNodeKinds;
using Microsoft.OData.Core.UriParser.Visitors;
using MongoDB.Bson;
using MongoDB.Driver;
using Squidex.Core.Schemas;

// ReSharper disable InvertIf
// ReSharper disable SwitchStatementMissingSomeCases
// ReSharper disable ConvertIfStatementToSwitchStatement

namespace Squidex.Read.MongoDb.Contents.Visitors
{
    public class FilterVisitor : QueryNodeVisitor<FilterDefinition<MongoContentEntity>>
    {
        private static readonly FilterDefinitionBuilder<MongoContentEntity> Filter = Builders<MongoContentEntity>.Filter;
        private readonly Schema schema;

        private FilterVisitor(Schema schema)
        {
            this.schema = schema;
        }

        public static FilterDefinition<MongoContentEntity> Visit(QueryNode node, Schema schema)
        {
            var visitor = new FilterVisitor(schema);

            return node.Accept(visitor);
        }

        public override FilterDefinition<MongoContentEntity> Visit(UnaryOperatorNode nodeIn)
        {
            if (nodeIn.OperatorKind == UnaryOperatorKind.Not)
            {
                return Filter.Not(nodeIn.Operand.Accept(this));
            }

            throw new NotSupportedException();
        }

        public override FilterDefinition<MongoContentEntity> Visit(SingleValueFunctionCallNode nodeIn)
        {
            var fieldNode = nodeIn.Parameters.ElementAt(0);
            var valueNode = nodeIn.Parameters.ElementAt(1);

            if (nodeIn.Name == "endswith")
            {
                var value = BuildRegex(valueNode, v => v + "$");

                return Filter.Regex(BuildFieldDefinition(fieldNode), value);
            }
            if (nodeIn.Name == "startswith")
            {
                var value = BuildRegex(valueNode, v => "^" + v);

                return Filter.Regex(BuildFieldDefinition(fieldNode), value);
            }
            if (nodeIn.Name == "contains")
            {
                var value = BuildRegex(valueNode, v => v);

                return Filter.Regex(BuildFieldDefinition(fieldNode), value);
            }

            throw new NotSupportedException();
        }

        public override FilterDefinition<MongoContentEntity> Visit(BinaryOperatorNode nodeIn)
        {
            if (nodeIn.OperatorKind == BinaryOperatorKind.And)
            {
                return Filter.And(nodeIn.Left.Accept(this), nodeIn.Right.Accept(this));
            }
            if (nodeIn.OperatorKind == BinaryOperatorKind.Or)
            {
                return Filter.Or(nodeIn.Left.Accept(this), nodeIn.Right.Accept(this));
            }
            if (nodeIn.OperatorKind == BinaryOperatorKind.NotEqual)
            {
                return Filter.Ne(BuildFieldDefinition(nodeIn.Left), BuildValue(nodeIn.Right));
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

            throw new NotSupportedException();
        }

        private static BsonRegularExpression BuildRegex(QueryNode node, Func<string, string> formatter)
        {
            return new BsonRegularExpression(formatter(BuildValue(node).ToString()), "i");
        }

        private FieldDefinition<MongoContentEntity, object> BuildFieldDefinition(QueryNode nodeIn)
        {
            return PropertyVisitor.Visit(nodeIn, schema);
        }

        private static object BuildValue(QueryNode nodeIn)
        {
            return ConstantVisitor.Visit(nodeIn);
        }
    }
}
