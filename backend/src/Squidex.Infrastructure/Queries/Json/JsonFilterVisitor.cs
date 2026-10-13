// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Validation;

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Squidex.Infrastructure.Queries.Json;

public sealed class JsonFilterVisitor : FilterNodeVisitor<FilterNode<ClrValue>, JsonValue, JsonFilterVisitor.Args>
{
    private static readonly JsonFilterVisitor Instance = new JsonFilterVisitor();

    public record struct Args(QueryModel Model, List<string> Errors);

    private JsonFilterVisitor()
    {
    }

    public static FilterNode<ClrValue>? Parse(FilterNode<JsonValue> filter, QueryModel model, List<string> errors)
    {
        var args = new Args(model, errors);

        var parsed = filter.Accept(Instance, args);

        if (errors.Count > 0)
        {
            return null;
        }
        else
        {
            return parsed;
        }
    }

    public override FilterNode<ClrValue> Visit(NegateFilter<JsonValue> nodeIn, Args args)
    {
        return new NegateFilter<ClrValue>(nodeIn.Filter.Accept(this, args));
    }

    public override FilterNode<ClrValue> Visit(LogicalFilter<JsonValue> nodeIn, Args args)
    {
        return new LogicalFilter<ClrValue>(nodeIn.Type, nodeIn.Filters.Select(x => x.Accept(this, args)).ToList());
    }

    public override FilterNode<ClrValue> Visit(CompareFilter<JsonValue> nodeIn, Args args)
    {
        var fieldMatches = nodeIn.Path.GetMatchingFields(args.Model.Schema, args.Errors);
        var fieldErrors = new List<string>();

        foreach (var field in fieldMatches)
        {
            fieldErrors.Clear();

            var isValidOperator = args.Model.Operators.TryGetValue(field.Schema.Type, out var operators) && operators.Contains(nodeIn.Operator);

            if (!isValidOperator)
            {
                fieldErrors.Add(Errors.InvalidOperator(nodeIn.Operator, field.Schema.Type, nodeIn.Path));
            }

            var value = ValueConverter.Convert(field, nodeIn.Value, nodeIn.Path, fieldErrors);

            if (value != null && isValidOperator)
            {
                if (nodeIn.Operator == CompareOperator.In)
                {
                    if (!value.IsList)
                    {
                        value = value.ToList();
                    }
                }
                else
                {
                    if (value.IsList)
                    {
                        fieldErrors.Add(Errors.InvalidArray(nodeIn.Operator, nodeIn.Path));
                    }
                }

                if (nodeIn.Operator == CompareOperator.Matchs && value.Value?.ToString()?.IsValidRegex() != true)
                {
                    fieldErrors.Add(Errors.InvalidRegex(value.ToString(), nodeIn.Path));
                }
            }

            if (args.Errors.Count == 0 && fieldErrors.Count == 0 && value != null)
            {
                return CreateFilter(nodeIn, value);
            }
            else if (field == fieldMatches.Last())
            {
                args.Errors.AddRange(fieldErrors);
            }
        }

        return new CompareFilter<ClrValue>(nodeIn.Path, nodeIn.Operator, ClrValue.Null);
    }

    private static FilterNode<ClrValue> CreateFilter(CompareFilter<JsonValue> nodeIn, ClrValue value)
    {
        // Dates without time are converted to ranges to match the whole day.
        if (value.Value is Instant instant && nodeIn.Value.Value is string text && DateFilter.IsDate(text))
        {
            return DateFilter.Create(nodeIn.Path, nodeIn.Operator, instant);
        }

        if (value.Value is not List<Instant> instants)
        {
            return new CompareFilter<ClrValue>(nodeIn.Path, nodeIn.Operator, value);
        }

        var source = nodeIn.Value.Value as JsonArray ?? [nodeIn.Value];
        if (source.Count == instants.Count)
        {
            var dates = new List<Instant>();
            var others = new List<Instant>();

            for (var i = 0; i < source.Count; i++)
            {
                if (source[i].Value is string item && DateFilter.IsDate(item))
                {
                    dates.Add(instants[i]);
                }
                else
                {
                    others.Add(instants[i]);
                }
            }

            if (dates.Count > 0)
            {
                return DateFilter.CreateIn(nodeIn.Path, dates, others);
            }
        }

        return new CompareFilter<ClrValue>(nodeIn.Path, nodeIn.Operator, value);
    }
}
