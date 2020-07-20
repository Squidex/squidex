// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;

namespace Squidex.Infrastructure.Queries
{
    public sealed class NegateFilter<TValue> : FilterNode<TValue>
    {
        public FilterNode<TValue> Filter { get; }

        public NegateFilter(FilterNode<TValue> filter)
        {
            Guard.NotNull(filter, nameof(filter));

            Filter = filter;
        }

        public override void AddFields(HashSet<string> fields)
        {
            Filter.AddFields(fields);
        }

        public override T Accept<T>(FilterNodeVisitor<T, TValue> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return $"!({Filter})";
        }
    }
}
