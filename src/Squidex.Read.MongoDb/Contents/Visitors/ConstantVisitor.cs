// ==========================================================================
//  ConstantVisitor.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Microsoft.OData.Core.UriParser.Semantic;
using Microsoft.OData.Core.UriParser.Visitors;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;
using NodaTime;
using NodaTime.Text;

// ReSharper disable ConvertIfStatementToReturnStatement
// ReSharper disable InvertIf

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

        public override object Visit(ConvertNode nodeIn)
        {
            var booleanType = EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Boolean);

            if (nodeIn.TypeReference.Definition == booleanType)
            {
                return bool.Parse(Visit(nodeIn.Source).ToString());
            }

            var dateTimeType = EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.DateTimeOffset);

            if (nodeIn.TypeReference.Definition == dateTimeType)
            {
                var value = Visit(nodeIn.Source);

                if (value is DateTimeOffset)
                {
                    return Instant.FromDateTimeOffset((DateTimeOffset)value);
                }

                return InstantPattern.General.Parse(Visit(nodeIn.Source).ToString()).Value;
            }

            return base.Visit(nodeIn);
        }

        public override object Visit(ConstantNode nodeIn)
        {
            return nodeIn.Value;
        }
    }
}
