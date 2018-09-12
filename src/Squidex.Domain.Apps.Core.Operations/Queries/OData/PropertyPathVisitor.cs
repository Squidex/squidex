// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Immutable;
using Microsoft.OData.UriParser;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Queries
{
    public sealed class PropertyPathVisitor : QueryNodeVisitor<ImmutableList<string>>
    {
        private static readonly PropertyPathVisitor Instance = new PropertyPathVisitor();

        private PropertyPathVisitor()
        {
        }

        public static ImmutableList<string> Visit(QueryNode node)
        {
            return node.Accept(Instance);
        }

        public override ImmutableList<string> Visit(ConvertNode nodeIn)
        {
            return nodeIn.Source.Accept(this);
        }

        public override ImmutableList<string> Visit(SingleComplexNode nodeIn)
        {
            if (nodeIn.Source is SingleComplexNode)
            {
                return nodeIn.Source.Accept(this).Add(nodeIn.Property.Name.ToPascalCase());
            }
            else
            {
                return ImmutableList.Create(nodeIn.Property.Name.ToPascalCase());
            }
        }

        public override ImmutableList<string> Visit(SingleValuePropertyAccessNode nodeIn)
        {
            if (nodeIn.Source is SingleComplexNode)
            {
                return nodeIn.Source.Accept(this).Add(nodeIn.Property.Name.ToPascalCase());
            }
            else
            {
                return ImmutableList.Create(nodeIn.Property.Name.ToPascalCase());
            }
        }
    }
}
