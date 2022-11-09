// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

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
        return new NegateFilter<ClrValue>(nodeIn.Accept(this, args));
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
                return new CompareFilter<ClrValue>(nodeIn.Path, nodeIn.Operator, value);
            }
            else if (field == fieldMatches.Last())
            {
                args.Errors.AddRange(fieldErrors);
            }
        }

        return new CompareFilter<ClrValue>(nodeIn.Path, nodeIn.Operator, ClrValue.Null);
    }
}
