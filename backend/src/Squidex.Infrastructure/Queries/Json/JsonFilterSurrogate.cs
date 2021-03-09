// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Infrastructure.Queries
{
    public sealed class JsonFilterSurrogate : ISurrogate<FilterNode<IJsonValue>>
    {
        public FilterNode<IJsonValue>[]? And { get; set; }

        public FilterNode<IJsonValue>[]? Or { get; set; }

        public FilterNode<IJsonValue>? Not { get; set; }

        public string? Op { get; set; }

        public string? Path { get; set; }

        public IJsonValue? Value { get; set; }

        public void FromSource(FilterNode<IJsonValue> source)
        {
            throw new NotSupportedException();
        }

        public FilterNode<IJsonValue> ToSource()
        {
            if (Not != null)
            {
                return new NegateFilter<IJsonValue>(Not);
            }

            if (And != null)
            {
                return new LogicalFilter<IJsonValue>(LogicalFilterType.And, And);
            }

            if (Or != null)
            {
                return new LogicalFilter<IJsonValue>(LogicalFilterType.Or, Or);
            }

            if (!string.IsNullOrWhiteSpace(Path) && !string.IsNullOrWhiteSpace(Op))
            {
                var @operator = ReadOperator(Op);

                return new CompareFilter<IJsonValue>(Path, @operator, Value ?? JsonValue.Null);
            }

            throw new JsonException("Invalid query.");
        }

        private static CompareOperator ReadOperator(string op)
        {
            switch (op.ToLowerInvariant())
            {
                case "eq":
                    return CompareOperator.Equals;
                case "ne":
                    return CompareOperator.NotEquals;
                case "lt":
                    return CompareOperator.LessThan;
                case "le":
                    return CompareOperator.LessThanOrEqual;
                case "gt":
                    return CompareOperator.GreaterThan;
                case "ge":
                    return CompareOperator.GreaterThanOrEqual;
                case "empty":
                    return CompareOperator.Empty;
                case "exists":
                    return CompareOperator.Exists;
                case "matchs":
                    return CompareOperator.Matchs;
                case "contains":
                    return CompareOperator.Contains;
                case "endswith":
                    return CompareOperator.EndsWith;
                case "startswith":
                    return CompareOperator.StartsWith;
                case "in":
                    return CompareOperator.In;
            }

            throw new JsonException($"Unexpected compare operator, got {op}.");
        }
    }
}
