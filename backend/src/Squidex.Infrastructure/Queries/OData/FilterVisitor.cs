// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using Microsoft.OData.UriParser;
using Microsoft.Spatial;

namespace Squidex.Infrastructure.Queries.OData
{
    public sealed class FilterVisitor : QueryNodeVisitor<FilterNode<ClrValue>>
    {
        private static readonly FilterVisitor Instance = new FilterVisitor();

        private FilterVisitor()
        {
        }

        public static FilterNode<ClrValue> Visit(QueryNode node)
        {
            return node.Accept(Instance);
        }

        public override FilterNode<ClrValue> Visit(ConvertNode nodeIn)
        {
            return nodeIn.Source.Accept(this);
        }

        public override FilterNode<ClrValue> Visit(UnaryOperatorNode nodeIn)
        {
            if (nodeIn.OperatorKind == UnaryOperatorKind.Not)
            {
                return ClrFilter.Not(nodeIn.Operand.Accept(this));
            }

            throw new NotSupportedException();
        }

        public override FilterNode<ClrValue> Visit(InNode nodeIn)
        {
            var value = ConstantWithTypeVisitor.Visit(nodeIn.Right);

            return ClrFilter.In(PropertyPathVisitor.Visit(nodeIn.Left), value);
        }

        public override FilterNode<ClrValue> Visit(SingleValueFunctionCallNode nodeIn)
        {
            var fieldNode = nodeIn.Parameters.ElementAt(0);

            if (string.Equals(nodeIn.Name, "empty", StringComparison.OrdinalIgnoreCase))
            {
                return ClrFilter.Empty(PropertyPathVisitor.Visit(fieldNode));
            }

            if (string.Equals(nodeIn.Name, "empty", StringComparison.OrdinalIgnoreCase))
            {
                return ClrFilter.Empty(PropertyPathVisitor.Visit(fieldNode));
            }

            if (string.Equals(nodeIn.Name, "exists", StringComparison.OrdinalIgnoreCase))
            {
                return ClrFilter.Exists(PropertyPathVisitor.Visit(fieldNode));
            }

            var valueNode = nodeIn.Parameters.ElementAt(1);

            if (string.Equals(nodeIn.Name, "matchs", StringComparison.OrdinalIgnoreCase))
            {
                var value = ConstantWithTypeVisitor.Visit(valueNode);

                return ClrFilter.Matchs(PropertyPathVisitor.Visit(fieldNode), value);
            }

            if (string.Equals(nodeIn.Name, "endswith", StringComparison.OrdinalIgnoreCase))
            {
                var value = ConstantWithTypeVisitor.Visit(valueNode);

                return ClrFilter.EndsWith(PropertyPathVisitor.Visit(fieldNode), value);
            }

            if (string.Equals(nodeIn.Name, "startswith", StringComparison.OrdinalIgnoreCase))
            {
                var value = ConstantWithTypeVisitor.Visit(valueNode);

                return ClrFilter.StartsWith(PropertyPathVisitor.Visit(fieldNode), value);
            }

            if (string.Equals(nodeIn.Name, "contains", StringComparison.OrdinalIgnoreCase))
            {
                var value = ConstantWithTypeVisitor.Visit(valueNode);

                return ClrFilter.Contains(PropertyPathVisitor.Visit(fieldNode), value);
            }

            throw new NotSupportedException();
        }

        public override FilterNode<ClrValue> Visit(BinaryOperatorNode nodeIn)
        {
            if (nodeIn.OperatorKind == BinaryOperatorKind.And)
            {
                return ClrFilter.And(nodeIn.Left.Accept(this), nodeIn.Right.Accept(this));
            }

            if (nodeIn.OperatorKind == BinaryOperatorKind.Or)
            {
                return ClrFilter.Or(nodeIn.Left.Accept(this), nodeIn.Right.Accept(this));
            }

            if (nodeIn.Left is SingleValueFunctionCallNode functionNode)
            {
                if (string.Equals(functionNode.Name, "geo.distance", StringComparison.OrdinalIgnoreCase) && nodeIn.OperatorKind == BinaryOperatorKind.LessThan)
                {
                    var valueDistance = (double)ConstantWithTypeVisitor.Visit(nodeIn.Right).Value!;

                    if (functionNode.Parameters.ElementAt(1) is not ConstantNode constantNode)
                    {
                        throw new NotSupportedException();
                    }

                    if (constantNode.Value is not GeographyPoint geographyPoint)
                    {
                        throw new NotSupportedException();
                    }

                    var property = PropertyPathVisitor.Visit(functionNode.Parameters.ElementAt(0));

                    return ClrFilter.Lt(property, new FilterSphere(geographyPoint.Longitude, geographyPoint.Latitude, valueDistance));
                }
                else
                {
                    var regexFilter = Visit(functionNode);

                    var value = ConstantWithTypeVisitor.Visit(nodeIn.Right);

                    if (value.ValueType == ClrValueType.Boolean && value.Value is bool booleanRight)
                    {
                        if ((nodeIn.OperatorKind == BinaryOperatorKind.Equal && !booleanRight) ||
                            (nodeIn.OperatorKind == BinaryOperatorKind.NotEqual && booleanRight))
                        {
                            regexFilter = ClrFilter.Not(regexFilter);
                        }

                        return regexFilter;
                    }
                }
            }
            else
            {
                if (nodeIn.OperatorKind == BinaryOperatorKind.NotEqual)
                {
                    var value = ConstantWithTypeVisitor.Visit(nodeIn.Right);

                    return ClrFilter.Ne(PropertyPathVisitor.Visit(nodeIn.Left), value);
                }

                if (nodeIn.OperatorKind == BinaryOperatorKind.Equal)
                {
                    var value = ConstantWithTypeVisitor.Visit(nodeIn.Right);

                    return ClrFilter.Eq(PropertyPathVisitor.Visit(nodeIn.Left), value);
                }

                if (nodeIn.OperatorKind == BinaryOperatorKind.LessThan)
                {
                    var value = ConstantWithTypeVisitor.Visit(nodeIn.Right);

                    return ClrFilter.Lt(PropertyPathVisitor.Visit(nodeIn.Left), value);
                }

                if (nodeIn.OperatorKind == BinaryOperatorKind.LessThanOrEqual)
                {
                    var value = ConstantWithTypeVisitor.Visit(nodeIn.Right);

                    return ClrFilter.Le(PropertyPathVisitor.Visit(nodeIn.Left), value);
                }

                if (nodeIn.OperatorKind == BinaryOperatorKind.GreaterThan)
                {
                    var value = ConstantWithTypeVisitor.Visit(nodeIn.Right);

                    return ClrFilter.Gt(PropertyPathVisitor.Visit(nodeIn.Left), value);
                }

                if (nodeIn.OperatorKind == BinaryOperatorKind.GreaterThanOrEqual)
                {
                    var value = ConstantWithTypeVisitor.Visit(nodeIn.Right);

                    return ClrFilter.Ge(PropertyPathVisitor.Visit(nodeIn.Left), value);
                }
            }

            throw new NotSupportedException();
        }
    }
}
