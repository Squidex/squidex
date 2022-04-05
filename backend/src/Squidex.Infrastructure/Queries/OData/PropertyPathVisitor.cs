// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Immutable;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Squidex.Infrastructure.Queries.OData
{
    public sealed class PropertyPathVisitor : QueryNodeVisitor<ImmutableList<string>>
    {
        private static readonly PropertyPathVisitor Instance = new PropertyPathVisitor();

        private PropertyPathVisitor()
        {
        }

        public static PropertyPath Visit(QueryNode node)
        {
            return new PropertyPath(node.Accept(Instance));
        }

        public override ImmutableList<string> Visit(ConvertNode nodeIn)
        {
            return nodeIn.Source.Accept(this);
        }

        public override ImmutableList<string> Visit(SingleComplexNode nodeIn)
        {
            if (nodeIn.Source is SingleComplexNode)
            {
                return nodeIn.Source.Accept(this).Add(UnescapeEdmField(nodeIn.Property));
            }
            else
            {
                return ImmutableList.Create(UnescapeEdmField(nodeIn.Property));
            }
        }

        public override ImmutableList<string> Visit(SingleValuePropertyAccessNode nodeIn)
        {
            if (nodeIn.Source is SingleComplexNode)
            {
                return nodeIn.Source.Accept(this).Add(UnescapeEdmField(nodeIn.Property));
            }
            else
            {
                return ImmutableList.Create(UnescapeEdmField(nodeIn.Property));
            }
        }

        public override ImmutableList<string> Visit(SingleValueOpenPropertyAccessNode nodeIn)
        {
            return nodeIn.Source.Accept(this).Add(nodeIn.Name);
        }

        private static string UnescapeEdmField(IEdmNamedElement property)
        {
            return property.Name;
        }
    }
}
