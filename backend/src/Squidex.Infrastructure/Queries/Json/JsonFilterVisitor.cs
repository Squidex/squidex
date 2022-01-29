// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Validation;

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Squidex.Infrastructure.Queries.Json
{
    public sealed class JsonFilterVisitor : FilterNodeVisitor<FilterNode<ClrValue>, IJsonValue, JsonFilterVisitor.Args>
    {
        private static readonly JsonFilterVisitor Instance = new JsonFilterVisitor();

        public record struct Args(QueryModel Model, List<string> Errors);

        private JsonFilterVisitor()
        {
        }

        public static FilterNode<ClrValue>? Parse(FilterNode<IJsonValue> filter, QueryModel model, List<string> errors)
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

        public override FilterNode<ClrValue> Visit(NegateFilter<IJsonValue> nodeIn, Args args)
        {
            return new NegateFilter<ClrValue>(nodeIn.Accept(this, args));
        }

        public override FilterNode<ClrValue> Visit(LogicalFilter<IJsonValue> nodeIn, Args args)
        {
            return new LogicalFilter<ClrValue>(nodeIn.Type, nodeIn.Filters.Select(x => x.Accept(this, args)).ToList());
        }

        public override FilterNode<ClrValue> Visit(CompareFilter<IJsonValue> nodeIn, Args args)
        {
            CompareFilter<ClrValue>? result = null;

            if (nodeIn.Path.TryGetField(args.Model, args.Errors, out var field))
            {
                var isValidOperator = args.Model.Operators.TryGetValue(field.Schema.Type, out var operators) && operators.Contains(nodeIn.Operator);

                if (!isValidOperator)
                {
                    var name = field.Schema.Type.ToString();

                    args.Errors.Add($"'{nodeIn.Operator}' is not a valid operator for type {name} at '{nodeIn.Path}'.");
                }

                var value = ValueConverter.Convert(field, nodeIn.Value, nodeIn.Path, args.Errors);

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
                            args.Errors.Add($"Array value is not allowed for '{nodeIn.Operator}' operator and path '{nodeIn.Path}'.");
                        }
                    }

                    if (nodeIn.Operator == CompareOperator.Matchs && value.Value?.ToString()?.IsValidRegex() != true)
                    {
                        args.Errors.Add($"{value} is not a valid regular expression.");
                    }

                    result = new CompareFilter<ClrValue>(nodeIn.Path, nodeIn.Operator, value);
                }
            }

            result ??= new CompareFilter<ClrValue>(nodeIn.Path, nodeIn.Operator, ClrValue.Null);

            return result;
        }
    }
}
