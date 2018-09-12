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

namespace Squidex.Domain.Apps.Core.Queries
{
    public sealed class ConstantVisitor : QueryNodeVisitor<(object Value, FilterValueType ValueType)>
    {
        private static readonly IEdmPrimitiveType BooleanType = EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Boolean);
        private static readonly IEdmPrimitiveType DateTimeType = EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.DateTimeOffset);
        private static readonly IEdmPrimitiveType DoubleType = EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Double);
        private static readonly IEdmPrimitiveType GuidType = EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Guid);
        private static readonly IEdmPrimitiveType Int32Type = EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Int32);
        private static readonly IEdmPrimitiveType Int64Type = EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Int64);
        private static readonly IEdmPrimitiveType SingleType = EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Single);
        private static readonly IEdmPrimitiveType StringType = EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.String);

        private static readonly ConstantVisitor Instance = new ConstantVisitor();

        private ConstantVisitor()
        {
        }

        public static (object Value, FilterValueType ValueType) Visit(QueryNode node)
        {
            return node.Accept(Instance);
        }

        public override (object Value, FilterValueType ValueType) Visit(ConvertNode nodeIn)
        {
            if (nodeIn.TypeReference.Definition == BooleanType)
            {
                return (bool.Parse(Visit(nodeIn.Source).ToString()), FilterValueType.Boolean);
            }

            if (nodeIn.TypeReference.Definition == GuidType)
            {
                return (Guid.Parse(Visit(nodeIn.Source).ToString()), FilterValueType.Guid);
            }

            if (nodeIn.TypeReference.Definition == DateTimeType)
            {
                var value = Visit(nodeIn.Source);

                if (value.Value is DateTimeOffset dateTimeOffset)
                {
                    return (Instant.FromDateTimeOffset(dateTimeOffset), FilterValueType.Instant);
                }

                if (value.Value is DateTime dateTime)
                {
                    return (Instant.FromDateTimeUtc(DateTime.SpecifyKind(dateTime, DateTimeKind.Utc)), FilterValueType.Instant);
                }

                if (value.Value is Date date)
                {
                    return (Instant.FromUtc(date.Year, date.Month, date.Day, 0, 0), FilterValueType.Instant);
                }

                var parseResult = InstantPattern.General.Parse(Visit(nodeIn.Source).ToString());

                if (!parseResult.Success)
                {
                    throw new ODataException("Datetime is not in a valid format. Use ISO 8601");
                }

                return (parseResult.Value, FilterValueType.Instant);
            }

            return base.Visit(nodeIn);
        }

        public override (object Value, FilterValueType ValueType) Visit(ConstantNode nodeIn)
        {
            if (nodeIn.TypeReference == BooleanType)
            {
                return (nodeIn.Value, FilterValueType.Boolean);
            }

            if (nodeIn.TypeReference == DoubleType)
            {
                return (nodeIn.Value, FilterValueType.Double);
            }

            if (nodeIn.TypeReference == Int32Type)
            {
                return (nodeIn.Value, FilterValueType.Int32);
            }

            if (nodeIn.TypeReference == Int32Type)
            {
                return (nodeIn.Value, FilterValueType.Int64);
            }

            if (nodeIn.TypeReference == StringType)
            {
                return (nodeIn.Value, FilterValueType.String);
            }

            throw new NotSupportedException();
        }
    }
}
