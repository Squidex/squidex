// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using NJsonSchema;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Infrastructure.Queries.Json
{
    public sealed class JsonFilterVisitor : FilterNodeVisitor<FilterNode<ClrValue>, IJsonValue, JsonFilterVisitor.Args>
    {
        private static readonly JsonFilterVisitor Instance = new JsonFilterVisitor();

        public struct Args
        {
            public readonly List<string> Errors;

            public JsonSchema Schema;

            public Args(JsonSchema schema, List<string> errors)
            {
                Schema = schema;

                Errors = errors;
            }
        }

        private JsonFilterVisitor()
        {
        }

        public static FilterNode<ClrValue>? Parse(FilterNode<IJsonValue> filter, JsonSchema schema, List<string> errors)
        {
            var args = new Args(schema, errors);

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

            if (nodeIn.Path.TryGetProperty(args.Schema, args.Errors, out var property))
            {
                var isValidOperator = OperatorValidator.IsAllowedOperator(property, nodeIn.Operator);

                if (!isValidOperator)
                {
                    var name = property.Type.ToString();

                    if (!string.IsNullOrWhiteSpace(property.Format))
                    {
                        name = $"{name}({property.Format})";
                    }

                    args.Errors.Add($"'{nodeIn.Operator}' is not a valid operator for type {name} at '{nodeIn.Path}'.");
                }

                var value = ValueConverter.Convert(property, nodeIn.Value, nodeIn.Path, args.Errors);

                if (value != null && isValidOperator)
                {
                    if (value.IsList && nodeIn.Operator != CompareOperator.In)
                    {
                        args.Errors.Add($"Array value is not allowed for '{nodeIn.Operator}' operator and path '{nodeIn.Path}'.");
                    }

                    result = new CompareFilter<ClrValue>(nodeIn.Path, nodeIn.Operator, value);
                }
            }

            result ??= new CompareFilter<ClrValue>(nodeIn.Path, nodeIn.Operator, ClrValue.Null);

            return result;
        }
    }
}
