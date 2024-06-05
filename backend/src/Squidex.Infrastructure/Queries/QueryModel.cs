// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.Queries;

public sealed class QueryModel
{
    public static readonly IReadOnlyDictionary<FilterSchemaType, IReadOnlyList<CompareOperator>> DefaultOperators = new Dictionary<FilterSchemaType, IReadOnlyList<CompareOperator>>
    {
        [FilterSchemaType.Any] = Enum.GetValues(typeof(CompareOperator)).OfType<CompareOperator>().ToList(),
        [FilterSchemaType.Boolean] =
        [
            CompareOperator.Equals,
            CompareOperator.Exists,
            CompareOperator.In,
            CompareOperator.NotEquals
        ],
        [FilterSchemaType.DateTime] =
        [
            CompareOperator.Contains,
            CompareOperator.Empty,
            CompareOperator.Exists,
            CompareOperator.EndsWith,
            CompareOperator.Equals,
            CompareOperator.GreaterThan,
            CompareOperator.GreaterThanOrEqual,
            CompareOperator.In,
            CompareOperator.LessThan,
            CompareOperator.LessThanOrEqual,
            CompareOperator.Matchs,
            CompareOperator.NotEquals,
            CompareOperator.StartsWith
        ],
        [FilterSchemaType.GeoObject] =
        [
            CompareOperator.LessThan,
            CompareOperator.Exists
        ],
        [FilterSchemaType.Guid] =
        [
            CompareOperator.Contains,
            CompareOperator.Empty,
            CompareOperator.Exists,
            CompareOperator.EndsWith,
            CompareOperator.Equals,
            CompareOperator.GreaterThan,
            CompareOperator.GreaterThanOrEqual,
            CompareOperator.In,
            CompareOperator.LessThan,
            CompareOperator.LessThanOrEqual,
            CompareOperator.Matchs,
            CompareOperator.NotEquals,
            CompareOperator.StartsWith
        ],
        [FilterSchemaType.Object] = [],
        [FilterSchemaType.ObjectArray] =
        [
            CompareOperator.Empty,
            CompareOperator.Exists,
            CompareOperator.Equals,
            CompareOperator.In,
            CompareOperator.NotEquals
        ],
        [FilterSchemaType.Number] =
        [
            CompareOperator.Equals,
            CompareOperator.Exists,
            CompareOperator.LessThan,
            CompareOperator.LessThanOrEqual,
            CompareOperator.GreaterThan,
            CompareOperator.GreaterThanOrEqual,
            CompareOperator.In,
            CompareOperator.NotEquals
        ],
        [FilterSchemaType.String] =
        [
            CompareOperator.Contains,
            CompareOperator.Empty,
            CompareOperator.Exists,
            CompareOperator.EndsWith,
            CompareOperator.Equals,
            CompareOperator.GreaterThan,
            CompareOperator.GreaterThanOrEqual,
            CompareOperator.In,
            CompareOperator.LessThan,
            CompareOperator.LessThanOrEqual,
            CompareOperator.Matchs,
            CompareOperator.NotEquals,
            CompareOperator.StartsWith
        ],
        [FilterSchemaType.StringArray] =
        [
            CompareOperator.Contains,
            CompareOperator.Empty,
            CompareOperator.Exists,
            CompareOperator.EndsWith,
            CompareOperator.Equals,
            CompareOperator.GreaterThan,
            CompareOperator.GreaterThanOrEqual,
            CompareOperator.In,
            CompareOperator.LessThan,
            CompareOperator.LessThanOrEqual,
            CompareOperator.Matchs,
            CompareOperator.NotEquals,
            CompareOperator.StartsWith
        ]
    };

    public FilterSchema Schema { get; init; } = FilterSchema.Any;

    public IReadOnlyDictionary<FilterSchemaType, IReadOnlyList<CompareOperator>> Operators { get; init; } = DefaultOperators;

    public QueryModel Flatten(int maxDepth = 7, bool onlyWithOperators = true)
    {
        var predicate = (Predicate<FilterSchema>?)null;

        if (onlyWithOperators)
        {
            predicate = x => Operators.TryGetValue(x.Type, out var operators) && operators.Count > 0;
        }

        var flatten = Schema.Flatten(maxDepth, predicate);

        if (ReferenceEquals(flatten, Schema))
        {
            return this;
        }

        return new QueryModel { Operators = Operators, Schema = flatten };
    }
}
