// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.RegularExpressions;
using Microsoft.OData.UriParser;
using Microsoft.Spatial;
using NodaTime;

namespace Squidex.Infrastructure.Queries.OData;

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

        ThrowHelper.NotSupportedException();
        return default!;
    }

    public override FilterNode<ClrValue> Visit(InNode nodeIn)
    {
        var path = PropertyPathVisitor.Visit(nodeIn.Left);

        var value = ConstantWithTypeVisitor.Visit(nodeIn.Right);

        if (value.Value is not List<Instant> instants || nodeIn.Right is not CollectionConstantNode collection)
        {
            return ClrFilter.In(path, value);
        }

        // Items are converted to the target type and lose their original literal, therefore we parse the collection literal.
        var literals = collection.LiteralText?.Trim('(', ')').Split(',', StringSplitOptions.TrimEntries);
        if (literals?.Length != instants.Count)
        {
            return ClrFilter.In(path, value);
        }

        var dates = new List<Instant>();
        var others = new List<Instant>();

        for (var i = 0; i < instants.Count; i++)
        {
            if (DateFilter.IsDate(literals[i]))
            {
                dates.Add(instants[i]);
            }
            else
            {
                others.Add(instants[i]);
            }
        }

        if (dates.Count == 0)
        {
            return ClrFilter.In(path, value);
        }

        return DateFilter.CreateIn(path, dates, others);
    }

    public override FilterNode<ClrValue> Visit(SingleValueFunctionCallNode nodeIn)
    {
        var fieldNode = nodeIn.Parameters.ElementAtOrDefault(0);
        if (fieldNode == null)
        {
            ThrowHelper.NotSupportedException();
            return default!;
        }

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

        var valueNode = nodeIn.Parameters.ElementAtOrDefault(1);
        if (valueNode == null)
        {
            ThrowHelper.NotSupportedException();
            return default!;
        }

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

        ThrowHelper.NotSupportedException();
        return default!;
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
            if (string.Equals(functionNode.Name, "geo.distance", StringComparison.OrdinalIgnoreCase))
            {
                return ParseGeoDistance(nodeIn, functionNode);
            }

            if (string.Equals(functionNode.Name, "toupper", StringComparison.OrdinalIgnoreCase))
            {
                return ParseMatch(nodeIn, functionNode, c => !char.IsLetter(c) || char.IsUpper(c));
            }

            if (string.Equals(functionNode.Name, "tolower", StringComparison.OrdinalIgnoreCase))
            {
                return ParseMatch(nodeIn, functionNode, c => !char.IsLetter(c) || char.IsLower(c));
            }

            var innerFunction = Visit(functionNode);

            var value = ConstantWithTypeVisitor.Visit(nodeIn.Right);
            if (value.ValueType == ClrValueType.Boolean && value.Value is bool booleanRight)
            {
                if ((nodeIn.OperatorKind == BinaryOperatorKind.Equal && !booleanRight) ||
                    (nodeIn.OperatorKind == BinaryOperatorKind.NotEqual && booleanRight))
                {
                    innerFunction = ClrFilter.Not(innerFunction);
                }

                return innerFunction;
            }
        }
        else
        {
            if (nodeIn.OperatorKind == BinaryOperatorKind.NotEqual)
            {
                return Compare(nodeIn, CompareOperator.NotEquals);
            }

            if (nodeIn.OperatorKind == BinaryOperatorKind.Equal)
            {
                return Compare(nodeIn, CompareOperator.Equals);
            }

            if (nodeIn.OperatorKind == BinaryOperatorKind.LessThan)
            {
                return Compare(nodeIn, CompareOperator.LessThan);
            }

            if (nodeIn.OperatorKind == BinaryOperatorKind.LessThanOrEqual)
            {
                return Compare(nodeIn, CompareOperator.LessThanOrEqual);
            }

            if (nodeIn.OperatorKind == BinaryOperatorKind.GreaterThan)
            {
                return Compare(nodeIn, CompareOperator.GreaterThan);
            }

            if (nodeIn.OperatorKind == BinaryOperatorKind.GreaterThanOrEqual)
            {
                return Compare(nodeIn, CompareOperator.GreaterThanOrEqual);
            }
        }

        ThrowHelper.NotSupportedException();
        return default!;
    }

    private static FilterNode<ClrValue> Compare(BinaryOperatorNode nodeIn, CompareOperator @operator)
    {
        var path = PropertyPathVisitor.Visit(nodeIn.Left);

        var value = ConstantWithTypeVisitor.Visit(nodeIn.Right);

        // Dates without time are converted to ranges to match the whole day.
        if (value.Value is Instant date && ConstantWithTypeVisitor.IsDate(nodeIn.Right))
        {
            return DateFilter.Create(path, @operator, date);
        }

        return new CompareFilter<ClrValue>(path, @operator, value);
    }

    private static FilterNode<ClrValue> ParseMatch(BinaryOperatorNode nodeIn, SingleValueFunctionCallNode functionNode, Func<char, bool> condition)
    {
        if (nodeIn.OperatorKind is not BinaryOperatorKind.Equal and not BinaryOperatorKind.NotEqual)
        {
            ThrowHelper.NotSupportedException();
            return default!;
        }

        var value = ConstantWithTypeVisitor.Visit(nodeIn.Right);
        if (value.Value is not string text || !text.All(condition))
        {
            ThrowHelper.NotSupportedException();
            return default!;
        }

        FilterNode<ClrValue> filter =
            ClrFilter.Matchs(
                PropertyPathVisitor.Visit(functionNode.Parameters.ElementAt(0)),
                $"/^{Regex.Escape(text)}$/i");

        if (nodeIn.OperatorKind == BinaryOperatorKind.NotEqual)
        {
            filter = ClrFilter.Not(filter);
        }

        return filter;
    }

    private static FilterNode<ClrValue> ParseGeoDistance(BinaryOperatorNode nodeIn, SingleValueFunctionCallNode functionNode)
    {
        if (nodeIn.OperatorKind != BinaryOperatorKind.LessThan)
        {
            ThrowHelper.NotSupportedException();
            return default!;
        }

        var valueDistance = (double)ConstantWithTypeVisitor.Visit(nodeIn.Right).Value!;

        if (functionNode.Parameters.ElementAt(1) is not ConstantNode constantNode)
        {
            ThrowHelper.NotSupportedException();
            return default!;
        }

        if (constantNode.Value is not GeographyPoint geographyPoint)
        {
            ThrowHelper.NotSupportedException();
            return default!;
        }

        var property = PropertyPathVisitor.Visit(functionNode.Parameters.ElementAt(0));

        return ClrFilter.Lt(property, new FilterSphere(geographyPoint.Longitude, geographyPoint.Latitude, valueDistance));
    }
}
