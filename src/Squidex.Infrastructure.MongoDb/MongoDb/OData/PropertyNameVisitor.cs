// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Immutable;
using Microsoft.OData.UriParser;

namespace Squidex.Infrastructure.MongoDb.OData
{
    public sealed class PropertyNameVisitor : QueryNodeVisitor<ImmutableList<string>>
    {
        public static readonly PropertyNameVisitor Instance = new PropertyNameVisitor();

        private PropertyNameVisitor()
        {
        }

        public override ImmutableList<string> Visit(ConvertNode nodeIn)
        {
            return nodeIn.Source.Accept(this);
        }

        public override ImmutableList<string> Visit(SingleComplexNode nodeIn)
        {
            if (nodeIn.Source is SingleComplexNode)
            {
                return nodeIn.Source.Accept(this).Add(nodeIn.Property.Name);
            }
            else
            {
                return ImmutableList.Create(nodeIn.Property.Name);
            }
        }

        public override ImmutableList<string> Visit(SingleValuePropertyAccessNode nodeIn)
        {
            if (nodeIn.Source is SingleComplexNode)
            {
                return nodeIn.Source.Accept(this).Add(nodeIn.Property.Name);
            }
            else
            {
                return ImmutableList.Create(nodeIn.Property.Name);
            }
        }
    }
}
