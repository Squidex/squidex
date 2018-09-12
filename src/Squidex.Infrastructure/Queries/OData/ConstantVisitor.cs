// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.OData.UriParser;

namespace Squidex.Infrastructure.Queries.OData
{
    public sealed class ConstantVisitor : QueryNodeVisitor<object>
    {
        private static readonly ConstantVisitor Instance = new ConstantVisitor();

        private ConstantVisitor()
        {
        }

        public static object Visit(QueryNode node)
        {
            return node.Accept(Instance);
        }

        public override object Visit(ConstantNode nodeIn)
        {
            return nodeIn.Value;
        }
    }
}
