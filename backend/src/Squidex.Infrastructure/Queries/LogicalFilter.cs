// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Squidex.Infrastructure.Queries
{
    public sealed record LogicalFilter<TValue>(LogicalFilterType Type, IReadOnlyList<FilterNode<TValue>> Filters) : FilterNode<TValue>
    {
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
