// ==========================================================================
//  SearchTermVisitor.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Microsoft.OData.Core.UriParser.Semantic;
using Microsoft.OData.Core.UriParser.TreeNodeKinds;
using Microsoft.OData.Core.UriParser.Visitors;

namespace Squidex.Domain.Apps.Read.MongoDb.Contents.Visitors
{
    public class SearchTermVisitor : QueryNodeVisitor<string>
    {
        private static readonly SearchTermVisitor Instance = new SearchTermVisitor();

        private SearchTermVisitor()
        {
        }

        public static object Visit(QueryNode node)
        {
            return node.Accept(Instance);
        }

        public override string Visit(BinaryOperatorNode nodeIn)
        {
            if (nodeIn.OperatorKind == BinaryOperatorKind.And)
            {
                return nodeIn.Left.Accept(this) + " " + nodeIn.Right.Accept(this);
            }

            throw new NotSupportedException();
        }

        public override string Visit(SearchTermNode nodeIn)
        {
            return nodeIn.Text;
        }
    }
}
