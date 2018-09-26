// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;

namespace Squidex.Infrastructure.Queries
{
    public sealed class PascalCasePathConverter : TransformVisitor
    {
        private static readonly PascalCasePathConverter Instance = new PascalCasePathConverter();

        private PascalCasePathConverter()
        {
        }

        public static FilterNode Transform(FilterNode node)
        {
            return node.Accept(Instance);
        }

        public override FilterNode Visit(FilterComparison nodeIn)
        {
            return new FilterComparison(nodeIn.Lhs.Select(x => x.ToPascalCase()).ToList(), nodeIn.Operator, nodeIn.Rhs);
        }
    }
}
