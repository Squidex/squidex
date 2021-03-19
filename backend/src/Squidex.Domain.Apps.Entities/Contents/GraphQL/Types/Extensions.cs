// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL;
using GraphQL.Types;
using Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Contents;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.ObjectPool;

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

                foreach (var (key, value) in context.Arguments)
                {
                    var formatted = value.Value?.ToString();

                    if (!string.IsNullOrWhiteSpace(formatted))
                    {
                        if (sb.Length > 1)
                        {
                            sb.Append('&');
                        }

                        sb.Append('$');
                        sb.Append(key);
                        sb.Append('=');
                        sb.Append(formatted);
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
            field.Metadata[key] = value;

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
    }
}
