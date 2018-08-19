// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Microsoft.OData;
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

                if (value is DateTime dateTime)
                {
                    return Instant.FromDateTimeUtc(DateTime.SpecifyKind(dateTime, DateTimeKind.Utc));
                }

                if (value is Date date)
                {
                    return Instant.FromUtc(date.Year, date.Month, date.Day, 0, 0);
                }

                var parseResult = InstantPattern.General.Parse(Visit(nodeIn.Source).ToString());

                if (!parseResult.Success)
                {
                    throw new ODataException("Datetime is not in a valid format. Use ISO 8601");
                }

                return parseResult.Value;
            }

            return base.Visit(nodeIn);
        }

        public override object Visit(ConstantNode nodeIn)
        {
            return nodeIn.Value;
        }
    }
}
