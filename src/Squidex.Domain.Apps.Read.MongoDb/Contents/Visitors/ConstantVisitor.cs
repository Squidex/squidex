// ==========================================================================
//  ConstantVisitor.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using NodaTime;
using NodaTime.Text;

namespace Squidex.Domain.Apps.Read.MongoDb.Contents.Visitors
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

                if (value is DateTimeOffset dateTimeOffset)
                {
                    return Instant.FromDateTimeOffset(dateTimeOffset);
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
