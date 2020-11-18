// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using GraphQL;
using GraphQL.Types;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Text;

#pragma warning disable RECS0015 // If an extension method is called as static method convert it to method syntax

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types
{
    public static class Extensions
    {
        public static string SafeTypeName(this string typeName)
        {
            if (typeName.Equals("Content", StringComparison.Ordinal))
            {
                return $"{typeName}Entity";
            }

            return typeName;
        }

        public static IEnumerable<(T Field, string Name, string Type)> SafeFields<T>(this IEnumerable<T> fields) where T : IField
        {
            var allFields =
                fields.FieldNames()
                    .GroupBy(x => x.Name)
                    .Select(g => g.Select((f, i) => (f.Field, f.Name.SafeString(i), f.Type.SafeString(i))))
                    .SelectMany(x => x);

            return allFields;
        }

        private static IEnumerable<(T Field, string Name, string Type)> FieldNames<T>(this IEnumerable<T> fields) where T : IField
        {
            return fields.ForApi(true).Select(field => (field, CasingExtensions.ToCamelCase(field.Name), field.TypeName()));
        }

        private static string SafeString(this string value, int index)
        {
            if (value.Length > 0 && !char.IsLetter(value[0]))
            {
                value = "gql_" + value;
            }

            if (index > 0)
            {
                return value + (index + 1);
            }

            return value;
        }

        public static string BuildODataQuery(this IResolveFieldContext context)
        {
            var odataQuery = "?" +
                string.Join("&",
                    context.Arguments
                        .Select(x => new { x.Key, Value = x.Value.ToString() }).Where(x => !string.IsNullOrWhiteSpace(x.Value))
                        .Select(x => $"${x.Key}={x.Value}"));

            return odataQuery;
        }

        public static FieldType WithSourceName(this FieldType field, object value)
        {
            field.Metadata["sourceName"] = value;

            return field;
        }

        public static string GetSourceName(this FieldType field)
        {
            return field.Metadata.GetOrAddDefault("sourceName") as string ?? field.Name;
        }

        public static IGraphType Flatten(this QueryArgument type)
        {
            return type.ResolvedType.Flatten();
        }

        public static IGraphType Flatten(this IGraphType type)
        {
            if (type is IProvideResolvedType provider)
            {
                return provider.ResolvedType.Flatten();
            }

            return type;
        }
    }
}
