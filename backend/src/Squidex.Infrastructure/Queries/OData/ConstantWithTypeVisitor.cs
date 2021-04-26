// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using NodaTime;
using NodaTime.Text;

namespace Squidex.Infrastructure.Queries.OData
{
    public sealed class ConstantWithTypeVisitor : QueryNodeVisitor<ClrValue>
    {
        private static readonly IEdmPrimitiveType BooleanType = EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Boolean);
        private static readonly IEdmPrimitiveType DateType = EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Date);
        private static readonly IEdmPrimitiveType DateTimeType = EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.DateTimeOffset);
        private static readonly IEdmPrimitiveType DoubleType = EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Double);
        private static readonly IEdmPrimitiveType GuidType = EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Guid);
        private static readonly IEdmPrimitiveType Int32Type = EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Int32);
        private static readonly IEdmPrimitiveType Int64Type = EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Int64);
        private static readonly IEdmPrimitiveType SingleType = EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Single);
        private static readonly IEdmPrimitiveType StringType = EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.String);

        private static readonly ConstantWithTypeVisitor Instance = new ConstantWithTypeVisitor();

        private ConstantWithTypeVisitor()
        {
        }

        public static ClrValue Visit(QueryNode node)
        {
            return node.Accept(Instance);
        }

        public override ClrValue Visit(ConvertNode nodeIn)
        {
            return nodeIn.Source.Accept(this);
        }

        public override ClrValue Visit(CollectionConstantNode nodeIn)
        {
            if (nodeIn.ItemType.Definition == DateTimeType || nodeIn.ItemType.Definition == DateType)
            {
                return nodeIn.Collection.Select(x => ParseInstant(x.Value)).ToList();
            }

            if (nodeIn.ItemType.Definition == GuidType)
            {
                return nodeIn.Collection.Select(x => (Guid)x.Value).ToList();
            }

            if (nodeIn.ItemType.Definition == BooleanType)
            {
                return nodeIn.Collection.Select(x => (bool)x.Value).ToList();
            }

            if (nodeIn.ItemType.Definition == SingleType)
            {
                return nodeIn.Collection.Select(x => (float)x.Value).ToList();
            }

            if (nodeIn.ItemType.Definition == DoubleType)
            {
                return nodeIn.Collection.Select(x => (double)x.Value).ToList();
            }

            if (nodeIn.ItemType.Definition == Int32Type)
            {
                return nodeIn.Collection.Select(x => (int)x.Value).ToList();
            }

            if (nodeIn.ItemType.Definition == Int64Type)
            {
                return nodeIn.Collection.Select(x => (long)x.Value).ToList();
            }

            if (nodeIn.ItemType.Definition == StringType)
            {
                return nodeIn.Collection.Select(x => (string)x.Value).ToList();
            }

            throw new NotSupportedException();
        }

        public override ClrValue Visit(ConstantNode nodeIn)
        {
            if (nodeIn.Value == null)
            {
                return ClrValue.Null;
            }

            if (nodeIn.TypeReference == null)
            {
                throw new NotSupportedException();
            }

            if (nodeIn.TypeReference.Definition == DateTimeType || nodeIn.TypeReference.Definition == DateType)
            {
                return ParseInstant(nodeIn.Value);
            }

            if (nodeIn.TypeReference.Definition == GuidType)
            {
                return (Guid)nodeIn.Value;
            }

            if (nodeIn.TypeReference.Definition == BooleanType)
            {
                return (bool)nodeIn.Value;
            }

            if (nodeIn.TypeReference.Definition == SingleType)
            {
                return (float)nodeIn.Value;
            }

            if (nodeIn.TypeReference.Definition == DoubleType)
            {
                return (double)nodeIn.Value;
            }

            if (nodeIn.TypeReference.Definition == Int32Type)
            {
                return (int)nodeIn.Value;
            }

            if (nodeIn.TypeReference.Definition == Int64Type)
            {
                return (long)nodeIn.Value;
            }

            if (nodeIn.TypeReference.Definition == StringType)
            {
                return (string)nodeIn.Value;
            }

            throw new NotSupportedException();
        }

        private static Instant ParseInstant(object value)
        {
            if (value is DateTimeOffset dateTimeOffset)
            {
                return Instant.FromDateTimeOffset(dateTimeOffset.Add(dateTimeOffset.Offset));
            }

            if (value is DateTime dateTime)
            {
                return Instant.FromDateTimeUtc(DateTime.SpecifyKind(dateTime, DateTimeKind.Utc));
            }

            if (value is Date date)
            {
                return Instant.FromUtc(date.Year, date.Month, date.Day, 0, 0);
            }

            var parseResult = InstantPattern.ExtendedIso.Parse(value.ToString()!);

            if (!parseResult.Success)
            {
                throw new ODataException("Datetime is not in a valid format. Use ISO 8601");
            }

            return parseResult.Value;
        }
    }
}
