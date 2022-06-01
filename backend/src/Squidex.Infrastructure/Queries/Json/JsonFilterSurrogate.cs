// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Infrastructure.Queries.Json
{
    public sealed class JsonFilterSurrogate : ISurrogate<FilterNode<JsonValue2>>
    {
        public FilterNode<JsonValue2>[]? And { get; set; }

        public FilterNode<JsonValue2>[]? Or { get; set; }

        public FilterNode<JsonValue2>? Not { get; set; }

        public CompareOperator? Op { get; set; }

        public string? Path { get; set; }

        public JsonValue2 Value { get; set; }

        public void FromSource(FilterNode<JsonValue2> source)
        {
            throw new NotSupportedException();
        }

        public FilterNode<JsonValue2> ToSource()
        {
            if (Not != null)
            {
                return new NegateFilter<JsonValue2>(Not);
            }

            if (And != null)
            {
                return new LogicalFilter<JsonValue2>(LogicalFilterType.And, And);
            }

            if (Or != null)
            {
                return new LogicalFilter<JsonValue2>(LogicalFilterType.Or, Or);
            }

            if (!string.IsNullOrWhiteSpace(Path))
            {
                return new CompareFilter<JsonValue2>(Path, Op ?? CompareOperator.Equals, Value);
            }

            throw new JsonException(Errors.InvalidJsonStructure());
        }
    }
}
