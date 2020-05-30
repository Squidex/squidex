// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;

namespace Squidex.Infrastructure.Queries
{
    public sealed class LogicalFilter<TValue> : FilterNode<TValue>
    {
        public IReadOnlyList<FilterNode<TValue>> Filters { get; }

        public LogicalFilterType Type { get; }

        public LogicalFilter(LogicalFilterType type, IReadOnlyList<FilterNode<TValue>> filters)
        {
            Guard.NotNull(filters, nameof(filters));
            Guard.Enum(type, nameof(type));

            Filters = filters;

            Type = type;
        }

        public override void AddFields(HashSet<string> fields)
        {
            foreach (var filter in Filters)
            {
                filter.AddFields(fields);
            }
        }

        public override T Accept<T>(FilterNodeVisitor<T, TValue> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return $"({string.Join(Type == LogicalFilterType.And ? " && " : " || ", Filters)})";
        }
    }
}
