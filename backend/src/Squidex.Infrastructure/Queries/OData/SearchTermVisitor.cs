// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.OData.UriParser;

namespace Squidex.Infrastructure.Queries.OData;

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

        ThrowHelper.NotSupportedException();
        return default!;
    }

    public override string Visit(SearchTermNode nodeIn)
    {
        return nodeIn.Text;
    }
}
