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

            var pathStack = new Stack<string>();

            void AddField(FilterableField field)
            {
                pathStack.Push(field.Path);

                if (Operators.TryGetValue(field.Type, out var operators) && operators.Count > 0)
                {
                    var path = string.Join('.', pathStack.Reverse());

                    result.Add(field with { Path = path });
                }

                if (field.Fields?.Count > 0 && pathStack.Count < 5)
                {
                    AddFields(field.Fields);
                }

                pathStack.Pop();
            }

            void AddFields(IEnumerable<FilterableField> source)
            {
                foreach (var field in source)
                {
                    AddField(field);
                }
            }

            AddFields(Fields);

            var simplified = result.GroupBy(x => new { x.Path, x.Type }).SingleGroups();

            return new QueryModel { Operators = Operators, Fields = simplified.ToList() };
        }
    }
}
