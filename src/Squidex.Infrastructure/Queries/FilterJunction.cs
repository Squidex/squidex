// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;

namespace Squidex.Infrastructure.Queries
{
    public sealed class FilterJunction : FilterNode
    {
        public IReadOnlyList<FilterNode> Operands { get; }

        public FilterJunctionType JunctionType { get; }

        public FilterJunction(FilterJunctionType junctionType, IReadOnlyList<FilterNode> operands)
        {
            Guard.NotNull(operands, nameof(operands));
            Guard.GreaterEquals(operands.Count, 2, nameof(operands.Count));
            Guard.Enum(junctionType, nameof(junctionType));

            Operands = operands;

            JunctionType = junctionType;
        }

        public FilterJunction(FilterJunctionType junctionType, params FilterNode[] operands)
            : this(junctionType, operands?.ToList())
        {
        }

        public override T Accept<T>(FilterNodeVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return $"({string.Join(JunctionType == FilterJunctionType.And ? " && " : " || ", Operands)})";
        }
    }
}
