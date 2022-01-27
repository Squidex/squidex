// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Collections;

namespace Squidex.Infrastructure.Queries
{
    public sealed class QueryModel
    {
        public static readonly IReadOnlyDictionary<FilterableFieldType, IReadOnlyList<CompareOperator>> DefaultOperators = new Dictionary<FilterableFieldType, IReadOnlyList<CompareOperator>>
        {
            [FilterableFieldType.Any] = Enum.GetValues(typeof(CompareOperator)).OfType<CompareOperator>().ToList(),
            [FilterableFieldType.Boolean] = new List<CompareOperator>
            {
                CompareOperator.Equals,
                CompareOperator.Exists,
                CompareOperator.In,
                CompareOperator.NotEquals
            },
            [FilterableFieldType.DateTime] = new List<CompareOperator>
            {
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
            },
            [FilterableFieldType.GeoObject] = new List<CompareOperator>
            {
                CompareOperator.LessThan,
                CompareOperator.Exists
            },
            [FilterableFieldType.Guid] = new List<CompareOperator>
            {
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
            },
            [FilterableFieldType.Object] = new List<CompareOperator>(),
            [FilterableFieldType.ObjectArray] = new List<CompareOperator>
            {
                CompareOperator.Empty,
                CompareOperator.Exists,
                CompareOperator.Equals,
                CompareOperator.In,
                CompareOperator.NotEquals
            },
            [FilterableFieldType.Number] = new List<CompareOperator>
            {
                CompareOperator.Equals,
                CompareOperator.Exists,
                CompareOperator.LessThan,
                CompareOperator.LessThanOrEqual,
                CompareOperator.GreaterThan,
                CompareOperator.GreaterThanOrEqual,
                CompareOperator.In,
                CompareOperator.NotEquals
            },
            [FilterableFieldType.String] = new List<CompareOperator>
            {
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
            },
            [FilterableFieldType.StringArray] = new List<CompareOperator>
            {
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
            }
        };

        public IReadOnlyList<FilterableField> Fields { get; init; } = ReadonlyList.Empty<FilterableField>();

        public IReadOnlyDictionary<FilterableFieldType, IReadOnlyList<CompareOperator>> Operators { get; init; } = DefaultOperators;

        public QueryModel Flatten()
        {
            if (Fields.Count == 0)
            {
                return this;
            }

            var result = new List<FilterableField>();

            var path = new Stack<string>();

            void AddField(FilterableField field)
            {
                path.Push(field.FieldPath);

                result.Add(field with
                {
                    FieldPath = string.Join('.', path.Reverse())
                });

                if (field.Fields?.Count > 0)
                {
                    AddFields(field.Fields);
                }

                path.Pop();
            }

            void AddFields(IEnumerable<FilterableField> source)
            {
                foreach (var field in source)
                {
                    AddField(field);
                }
            }

            var simplified = result.GroupBy(x => new { x.FieldPath, x.Type }).SingleGroups().OrderBy(x => x.FieldPath);

            return new QueryModel { Operators = Operators, Fields = simplified.ToList() };
        }
    }
}
