// ==========================================================================
//  ConstantVisitor.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Microsoft.OData.Core.UriParser.Semantic;
using Microsoft.OData.Core.UriParser.Visitors;

namespace Squidex.Read.MongoDb.Contents.Visitors
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
