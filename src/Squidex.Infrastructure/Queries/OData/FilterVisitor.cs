// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using Microsoft.OData.UriParser;

namespace Squidex.Infrastructure.Queries.OData
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

        public override FilterNode Visit(InNode nodeIn)
        {
            var value = ConstantWithTypeVisitor.Visit(nodeIn.Right);

            return new FilterComparison(PropertyPathVisitor.Visit(nodeIn.Left), FilterOperator.In, value);
        }

        public override FilterNode Visit(SingleValueFunctionCallNode nodeIn)
        {
            var fieldNode = nodeIn.Parameters.ElementAt(0);
            var valueNode = nodeIn.Parameters.ElementAt(1);

            if (string.Equals(nodeIn.Name, "endswith", StringComparison.OrdinalIgnoreCase))
            {
                var value = ConstantWithTypeVisitor.Visit(valueNode);

                return new FilterComparison(PropertyPathVisitor.Visit(fieldNode), FilterOperator.EndsWith, value);
            }

            if (string.Equals(nodeIn.Name, "startswith", StringComparison.OrdinalIgnoreCase))
            {
                var value = ConstantWithTypeVisitor.Visit(valueNode);

                return new FilterComparison(PropertyPathVisitor.Visit(fieldNode), FilterOperator.StartsWith, value);
            }

            if (string.Equals(nodeIn.Name, "contains", StringComparison.OrdinalIgnoreCase))
            {
                var value = ConstantWithTypeVisitor.Visit(valueNode);

                return new FilterComparison(PropertyPathVisitor.Visit(fieldNode), FilterOperator.Contains, value);
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

                var value = ConstantWithTypeVisitor.Visit(nodeIn.Right);

                if (value.ValueType == FilterValueType.Boolean && value.Value is bool booleanRight)
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
                    var value = ConstantWithTypeVisitor.Visit(nodeIn.Right);

                    return new FilterComparison(PropertyPathVisitor.Visit(nodeIn.Left), FilterOperator.NotEquals, value);
                }

                if (nodeIn.OperatorKind == BinaryOperatorKind.Equal)
                {
                    var value = ConstantWithTypeVisitor.Visit(nodeIn.Right);

                    return new FilterComparison(PropertyPathVisitor.Visit(nodeIn.Left), FilterOperator.Equals, value);
                }

                if (nodeIn.OperatorKind == BinaryOperatorKind.LessThan)
                {
                    var value = ConstantWithTypeVisitor.Visit(nodeIn.Right);

                    return new FilterComparison(PropertyPathVisitor.Visit(nodeIn.Left), FilterOperator.LessThan, value);
                }

                if (nodeIn.OperatorKind == BinaryOperatorKind.LessThanOrEqual)
                {
                    var value = ConstantWithTypeVisitor.Visit(nodeIn.Right);

                    return new FilterComparison(PropertyPathVisitor.Visit(nodeIn.Left), FilterOperator.LessThanOrEqual, value);
                }

                if (nodeIn.OperatorKind == BinaryOperatorKind.GreaterThan)
                {
                    var value = ConstantWithTypeVisitor.Visit(nodeIn.Right);

                    return new FilterComparison(PropertyPathVisitor.Visit(nodeIn.Left), FilterOperator.GreaterThan, value);
                }

                if (nodeIn.OperatorKind == BinaryOperatorKind.GreaterThanOrEqual)
                {
                    var value = ConstantWithTypeVisitor.Visit(nodeIn.Right);

                    return new FilterComparison(PropertyPathVisitor.Visit(nodeIn.Left), FilterOperator.GreaterThanOrEqual, value);
                }
            }

            throw new NotSupportedException();
        }
    }
}
