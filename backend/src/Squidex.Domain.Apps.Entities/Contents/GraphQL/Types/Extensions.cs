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
using GraphQL.Utilities;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.ObjectPool;
using Squidex.Text;
using GraphQLSchema = GraphQL.Types.Schema;

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
            var sb = DefaultPools.StringBuilder.Get();
            try
            {
                sb.Append('?');

                foreach (var argument in context.Arguments)
                {
                    var value = argument.Value?.ToString();

                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        if (sb.Length > 1)
                        {
                            sb.Append('&');
                        }

                        sb.Append('$');
                        sb.Append(argument.Key);
                        sb.Append('=');
                        sb.Append(value);
                    }
                }

                return sb.ToString();
            }
            finally
            {
                DefaultPools.StringBuilder.Return(sb);
            }
        }

        public static FieldType WithSourceName(this FieldType field, object value)
        {
            if (field is MetadataProvider metadataProvider)
            {
                metadataProvider.Metadata = new Dictionary<string, object>
                {
                    ["sourceName"] = value
                };
            }

            return field;
        }

        public static string GetSourceName(this FieldType field)
        {
            return field.GetMetadata("sourceName", string.Empty);
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

        public static void CleanupMetadata(this GraphQLSchema schema)
        {
            var targets = new HashSet<IProvideMetadata>(ReferenceEqualityComparer.Instance);

            foreach (var type in schema.AllTypes)
            {
                FindTargets(type, targets);
            }

            foreach (var target in targets.OfType<MetadataProvider>())
            {
                var metadata = target.Metadata;

                if (metadata != null && metadata.Count == 0)
                {
                    target.Metadata = null;
                }
            }
        }

        private static void FindTargets(IGraphType type, HashSet<IProvideMetadata> targets)
        {
            if (type == null)
            {
                return;
            }

            if (targets.Add(type))
            {
                if (type is IComplexGraphType complexType)
                {
                    foreach (var field in complexType.Fields)
                    {
                        targets.Add(field);

                        FindTargets(field.ResolvedType, targets);

                        if (field.Arguments != null)
                        {
                            foreach (var argument in field.Arguments)
                            {
                                targets.Add(argument);

                                FindTargets(argument.ResolvedType, targets);
                            }
                        }
                    }

                    if (type is IObjectGraphType objectGraphType && objectGraphType.ResolvedInterfaces != null)
                    {
                        foreach (var @interface in objectGraphType.ResolvedInterfaces)
                        {
                            FindTargets(@interface, targets);
                        }
                    }

                    if (type is IAbstractGraphType abstractGraphType && abstractGraphType.PossibleTypes != null)
                    {
                        foreach (var possibleType in abstractGraphType.PossibleTypes)
                        {
                            FindTargets(possibleType, targets);
                        }
                    }
                }

                if (type is IProvideResolvedType provideType)
                {
                    FindTargets(provideType.ResolvedType, targets);
                }
            }
        }
    }
}
