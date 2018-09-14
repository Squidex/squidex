// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.Queries
{
    public sealed class FilterNegate : FilterNode
    {
        public FilterNode Operand { get; }

        public FilterNegate(FilterNode operand)
        {
            Guard.NotNull(operand, nameof(operand));

            Operand = operand;
        }

        public override T Accept<T>(FilterNodeVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return $"!({Operand})";
        }
    }
}
