// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using Microsoft.OData.UriParser;

namespace Squidex.Domain.Apps.Core.Queries
{
    public sealed class FilterVisitor : QueryNodeVisitor<FilterNode>
    {
        private static readonly FilterVisitor Instance = new FilterVisitor();

        private FilterVisitor()
        {
        }

        public static FilterNode Visit(QueryNode node)
        {
            return node.Accept(Instance);
        }

        public override FilterNode Visit(ConvertNode nodeIn)
        {
            return nodeIn.Source.Accept(this);
        }

        public override FilterNode Visit(UnaryOperatorNode nodeIn)
        {
            if (nodeIn.OperatorKind == UnaryOperatorKind.Not)
            {
                return new FilterNegate(nodeIn.Operand.Accept(this));
            }

            throw new NotSupportedException();
        }

        public override FilterNode Visit(SingleValueFunctionCallNode nodeIn)
        {
            var fieldNode = nodeIn.Parameters.ElementAt(0);
            var valueNode = nodeIn.Parameters.ElementAt(1);

            if (string.Equals(nodeIn.Name, "endswith", StringComparison.OrdinalIgnoreCase))
            {
                var (value, valueType) = ConstantVisitor.Visit(valueNode);

                return new FilterComparison(PropertyPathVisitor.Visit(fieldNode), FilterOperator.EndsWith, value, valueType);
            }

            if (string.Equals(nodeIn.Name, "startswith", StringComparison.OrdinalIgnoreCase))
            {
                var (value, valueType) = ConstantVisitor.Visit(valueNode);

                return new FilterComparison(PropertyPathVisitor.Visit(fieldNode), FilterOperator.StartsWith, value, valueType);
            }

            if (string.Equals(nodeIn.Name, "contains", StringComparison.OrdinalIgnoreCase))
            {
                var (value, valueType) = ConstantVisitor.Visit(valueNode);

                return new FilterComparison(PropertyPathVisitor.Visit(fieldNode), FilterOperator.Contains, value, valueType);
            }

            throw new NotSupportedException();
        }

        public override FilterNode Visit(BinaryOperatorNode nodeIn)
        {
            if (nodeIn.OperatorKind == BinaryOperatorKind.And)
            {
                return new FilterJunction(FilterJunctionType.And, nodeIn.Left.Accept(this), nodeIn.Right.Accept(this));
            }

            if (nodeIn.OperatorKind == BinaryOperatorKind.Or)
            {
                return new FilterJunction(FilterJunctionType.Or, nodeIn.Left.Accept(this), nodeIn.Right.Accept(this));
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
                        regexFilter = new FilterNegate(regexFilter);
                    }

                    return regexFilter;
                }
            }
            else
            {
                if (nodeIn.OperatorKind == BinaryOperatorKind.NotEqual)
                {
                    var (value, valueType) = ConstantVisitor.Visit(nodeIn.Left);

                    return new FilterComparison(PropertyPathVisitor.Visit(nodeIn.Right), FilterOperator.NotEquals, value, valueType);
                }

                if (nodeIn.OperatorKind == BinaryOperatorKind.Equal)
                {
                    var (value, valueType) = ConstantVisitor.Visit(nodeIn.Left);

                    return new FilterComparison(PropertyPathVisitor.Visit(nodeIn.Right), FilterOperator.Equals, value, valueType);
                }

                if (nodeIn.OperatorKind == BinaryOperatorKind.LessThan)
                {
                    var (value, valueType) = ConstantVisitor.Visit(nodeIn.Left);

                    return new FilterComparison(PropertyPathVisitor.Visit(nodeIn.Right), FilterOperator.LessThan, value, valueType);
                }

                if (nodeIn.OperatorKind == BinaryOperatorKind.LessThanOrEqual)
                {
                    var (value, valueType) = ConstantVisitor.Visit(nodeIn.Left);

                    return new FilterComparison(PropertyPathVisitor.Visit(nodeIn.Right), FilterOperator.LessThanOrEqual, value, valueType);
                }

                if (nodeIn.OperatorKind == BinaryOperatorKind.GreaterThan)
                {
                    var (value, valueType) = ConstantVisitor.Visit(nodeIn.Left);

                    return new FilterComparison(PropertyPathVisitor.Visit(nodeIn.Right), FilterOperator.GreaterThan, value, valueType);
                }

                if (nodeIn.OperatorKind == BinaryOperatorKind.GreaterThanOrEqual)
                {
                    var (value, valueType) = ConstantVisitor.Visit(nodeIn.Left);

                    return new FilterComparison(PropertyPathVisitor.Visit(nodeIn.Right), FilterOperator.GreaterThanOrEqual, value, valueType);
                }
            }

            throw new NotSupportedException();
        }

        private object BuildValue(QueryNode nodeIn)
        {
            return ConstantVisitor.Visit(nodeIn);
        }
    }
}
