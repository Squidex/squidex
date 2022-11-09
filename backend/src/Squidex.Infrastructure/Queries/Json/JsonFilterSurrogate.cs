// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Infrastructure.Queries.Json;

public sealed class JsonFilterSurrogate : ISurrogate<FilterNode<JsonValue>>
{
    public FilterNode<JsonValue>[]? And { get; set; }

    public FilterNode<JsonValue>[]? Or { get; set; }

    public FilterNode<JsonValue>? Not { get; set; }

    public CompareOperator? Op { get; set; }

    public string? Path { get; set; }

    public JsonValue Value { get; set; }

    public void FromSource(FilterNode<JsonValue> source)
    {
        throw new NotSupportedException();
    }

    public FilterNode<JsonValue> ToSource()
    {
        if (Not != null)
        {
            return new NegateFilter<JsonValue>(Not);
        }

        if (And != null)
        {
            return new LogicalFilter<JsonValue>(LogicalFilterType.And, And);
        }

        if (Or != null)
        {
            return new LogicalFilter<JsonValue>(LogicalFilterType.Or, Or);
        }

        if (!string.IsNullOrWhiteSpace(Path))
        {
            return new CompareFilter<JsonValue>(Path, Op ?? CompareOperator.Equals, Value);
        }

        ThrowHelper.JsonException(Errors.InvalidJsonStructure());
        return default!;
    }
}
