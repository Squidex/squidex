// ==========================================================================
//  PropertyVisitor.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.OData.UriParser;
using MongoDB.Driver;
using Squidex.Domain.Apps.Core.Schemas;

// ReSharper disable InvertIf
// ReSharper disable RedundantIfElseBlock

namespace Squidex.Domain.Apps.Read.MongoDb.Contents.Visitors
{
    public sealed class PropertyVisitor : QueryNodeVisitor<ImmutableList<string>>
    {
        private static readonly PropertyVisitor Instance = new PropertyVisitor();

        public static StringFieldDefinition<MongoContentEntity, object> Visit(QueryNode node, Schema schema)
        {
            var propertyNames = node.Accept(Instance).ToArray();

            if (propertyNames.Length == 3)
            {
                var edmName = propertyNames[1].UnescapeEdmField();

                if (!schema.FieldsByName.TryGetValue(edmName, out Field field))
                {
                    throw new NotSupportedException();
                }

                propertyNames[1] = field.Id.ToString();
            }

            var propertyName = $"do.{string.Join(".", propertyNames.Skip(1))}";

            return new StringFieldDefinition<MongoContentEntity, object>(propertyName);
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
