// ==========================================================================
//  PropertyVisitor.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================
using System.Collections.Immutable;
using System.Linq;
using Microsoft.OData.UriParser;
using MongoDB.Driver;

namespace Squidex.Domain.Apps.Entities.MongoDb.Assets.Visitors
{
    public sealed class PropertyVisitor : QueryNodeVisitor<ImmutableList<string>>
    {
        private static readonly PropertyVisitor Instance = new PropertyVisitor();

        public static StringFieldDefinition<MongoAssetEntity, object> Visit(QueryNode node)
        {
            var propertyNames = node.Accept(Instance).ToArray();

            return new StringFieldDefinition<MongoAssetEntity, object>(propertyNames.First());
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
