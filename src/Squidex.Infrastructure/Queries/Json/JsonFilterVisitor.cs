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
    public sealed class JsonFilterVisitor : FilterNodeVisitor<FilterNode<ClrValue>, IJsonValue>
    {
        private readonly List<string> errors;
        private readonly JsonSchema schema;

        private JsonFilterVisitor(JsonSchema schema, List<string> errors)
        {
            this.schema = schema;

            this.errors = errors;
        }

        public static FilterNode<ClrValue>? Parse(FilterNode<IJsonValue> filter, JsonSchema schema, List<string> errors)
        {
            var visitor = new JsonFilterVisitor(schema, errors);

            var parsed = filter.Accept(visitor);

            if (visitor.errors.Count > 0)
            {
                return null;
            }
            else
            {
                return parsed;
            }
        }

        public override FilterNode<ClrValue> Visit(NegateFilter<IJsonValue> nodeIn)
        {
            return new NegateFilter<ClrValue>(nodeIn.Accept(this));
        }

        public override FilterNode<ClrValue> Visit(LogicalFilter<IJsonValue> nodeIn)
        {
            return new LogicalFilter<ClrValue>(nodeIn.Type, nodeIn.Filters.Select(x => x.Accept(this)).ToList());
        }

        public override FilterNode<ClrValue> Visit(CompareFilter<IJsonValue> nodeIn)
        {
            CompareFilter<ClrValue>? result = null;

            if (nodeIn.Path.TryGetProperty(schema, errors, out var property))
            {
                var isValidOperator = OperatorValidator.IsAllowedOperator(property, nodeIn.Operator);

                if (!isValidOperator)
                {
                    errors.Add($"{nodeIn.Operator} is not a valid operator for type {property.Type} at {nodeIn.Path}.");
                }

                var value = ValueConverter.Convert(property, nodeIn.Value, nodeIn.Path, errors);

                if (value != null && isValidOperator)
                {
                    if (value.IsList && nodeIn.Operator != CompareOperator.In)
                    {
                        errors.Add($"Array value is not allowed for '{nodeIn.Operator}' operator and path '{nodeIn.Path}'.");
                    }

                    result = new CompareFilter<ClrValue>(nodeIn.Path, nodeIn.Operator, value);
                }
            }

            result ??= new CompareFilter<ClrValue>(nodeIn.Path, nodeIn.Operator, ClrValue.Null);

            return result;
        }
    }
}
