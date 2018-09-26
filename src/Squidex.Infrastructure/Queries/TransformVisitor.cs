// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;

namespace Squidex.Infrastructure.Queries
{
    public abstract class TransformVisitor : FilterNodeVisitor<FilterNode>
    {
        public override FilterNode Visit(FilterComparison nodeIn)
        {
            return nodeIn;
        }

        public override FilterNode Visit(FilterJunction nodeIn)
        {
            return new FilterJunction(nodeIn.JunctionType, nodeIn.Operands.Select(x => x.Accept(this)).ToList());
        }

        public override FilterNode Visit(FilterNegate nodeIn)
        {
            return new FilterNegate(nodeIn.Operand.Accept(this));
        }
    }
}
