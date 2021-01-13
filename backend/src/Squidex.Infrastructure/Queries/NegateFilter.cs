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
    public sealed record NegateFilter<TValue>(FilterNode<TValue> Filter) : FilterNode<TValue>
    {
        public override void AddFields(HashSet<string> fields)
        {
            Filter.AddFields(fields);
        }

        public override T Accept<T, TArgs>(FilterNodeVisitor<T, TValue, TArgs> visitor, TArgs args)
        {
            return visitor.Visit(this, args);
        }

        public override string ToString()
        {
            return $"!({Filter})";
        }
    }
}
