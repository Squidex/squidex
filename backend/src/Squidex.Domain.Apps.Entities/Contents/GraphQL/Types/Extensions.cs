// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using GraphQL;
using GraphQL.Types;
using GraphQL.Utilities;
using Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Contents;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.ObjectPool;
using GraphQLSchema = GraphQL.Types.Schema;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types
{
    internal static class Extensions
    {
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

        public static FieldType WithSourceName(this FieldType field, string value)
        {
            return field.WithMetadata(nameof(SourceName), value);
        }

        public static FieldType WithSourceName(this FieldType field, FieldInfo value)
        {
            return field.WithMetadata(nameof(SourceName), value.Field.Name);
        }

        public static string SourceName(this FieldType field)
        {
            return field.GetMetadata<string>(nameof(SourceName));
        }

        public static FieldType WithSchemaId(this FieldType field, SchemaInfo value)
        {
            return field.WithMetadata(nameof(SchemaId), value.Schema.Id.ToString());
        }

        public static string SchemaId(this FieldType field)
        {
            return field.GetMetadata<string>(nameof(SchemaId));
        }

        public static FieldType WithSchemaNamedId(this FieldType field, SchemaInfo value)
        {
            return field.WithMetadata(nameof(SchemaNamedId), value.Schema.NamedId());
        }

        public static NamedId<DomainId> SchemaNamedId(this FieldType field)
        {
            return field.GetMetadata<NamedId<DomainId>>(nameof(SchemaNamedId));
        }

        private static FieldType WithMetadata(this FieldType field, string key, object value)
        {
            if (field is MetadataProvider metadataProvider)
            {
                if (metadataProvider.Metadata is Dictionary<string, object> dict)
                {
                    dict[key] = value;
                }
                else
                {
                    metadataProvider.Metadata = new Dictionary<string, object>
                    {
                        [key] = value
                    };
                }
            }

            return field;
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

                    if (type is IObjectGraphType { ResolvedInterfaces: { } } objectGraphType)
                    {
                        foreach (var @interface in objectGraphType.ResolvedInterfaces)
                        {
                            FindTargets(@interface, targets);
                        }
                    }

                    if (type is IAbstractGraphType { PossibleTypes: { } } abstractGraphType)
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
