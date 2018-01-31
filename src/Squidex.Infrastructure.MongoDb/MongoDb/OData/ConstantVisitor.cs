// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using NodaTime;
using NodaTime.Text;

namespace Squidex.Infrastructure.MongoDb.OData
{
    public sealed class ConstantVisitor : QueryNodeVisitor<object>
    {
        private static readonly IEdmPrimitiveType BooleanType = EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Boolean);
        private static readonly IEdmPrimitiveType DateTimeType = EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.DateTimeOffset);
        private static readonly IEdmPrimitiveType GuidType = EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Guid);

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
            if (nodeIn.TypeReference.Definition == BooleanType)
            {
                return bool.Parse(Visit(nodeIn.Source).ToString());
            }

            if (nodeIn.TypeReference.Definition == GuidType)
            {
                return Guid.Parse(Visit(nodeIn.Source).ToString());
            }

            if (nodeIn.TypeReference.Definition == DateTimeType)
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
