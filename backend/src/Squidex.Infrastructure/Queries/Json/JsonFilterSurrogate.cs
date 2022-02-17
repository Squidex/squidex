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
    public sealed class JsonFilterSurrogate : ISurrogate<FilterNode<IJsonValue>>
    {
        public FilterNode<IJsonValue>[]? And { get; set; }

        public FilterNode<IJsonValue>[]? Or { get; set; }

        public FilterNode<IJsonValue>? Not { get; set; }

        public CompareOperator? Op { get; set; }

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

            if (!string.IsNullOrWhiteSpace(Path))
            {
                return new CompareFilter<IJsonValue>(Path, Op ?? CompareOperator.Equals, Value ?? JsonValue.Null);
            }

            throw new JsonException(Errors.InvalidJsonStructure());
        }
    }
}
