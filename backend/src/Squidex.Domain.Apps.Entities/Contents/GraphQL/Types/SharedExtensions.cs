// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL;
using GraphQL.Types;
using GraphQL.Utilities;
using GraphQLParser;
using GraphQLParser.AST;
using Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Directives;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.ObjectPool;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types;

public static class SharedExtensions
{
    internal static string EscapePartition(this string value)
    {
        return value.Replace('-', '_');
    }

    internal static FieldType WithouthResolver(this FieldType source)
    {
        return new FieldType
        {
            Name = source.Name,
            ResolvedType = source.ResolvedType,
            Resolver = null,
            Description = source.Name
        };
    }

    internal static string BuildODataQuery(this IResolveFieldContext context)
    {
        var sb = DefaultPools.StringBuilder.Get();
        try
        {
            sb.Append('?');

            if (context.Arguments != null)
            {
                foreach (var (key, value) in context.Arguments)
                {
                    var formatted = value.Value?.ToString();

                    if (!string.IsNullOrWhiteSpace(formatted))
                    {
                        if (key == "search")
                        {
                            formatted = $"\"{formatted.Trim('"')}\"";
                        }

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
            }

            return sb.ToString();
        }
        finally
        {
            DefaultPools.StringBuilder.Return(sb);
        }
    }

    public static bool IsValidName(this string? name, NamedElement type)
    {
        try
        {
            NameValidator.ValidateDefault(name!, type);
            return true;
        }
        catch
        {
            return false;
        }
    }

    internal static string SourceName(this FieldType field)
    {
        if (field is FieldTypeWithSourceName typed)
        {
            return typed.SourceName;
        }

        return field.Name;
    }

    internal static DomainId SchemaId(this FieldType field)
    {
        if (field is FieldTypeWithSchemaId typed)
        {
            return typed.SchemaId;
        }

        throw new InvalidOperationException("Invalid field type");
    }

    internal static NamedId<DomainId> SchemaNamedId(this FieldType field)
    {
        if (field is FieldTypeWithSchemaNamedId typed)
        {
            return typed.SchemaId;
        }

        throw new InvalidOperationException("Invalid field type");
    }

    public static IGraphType? InnerType(this IGraphType type)
    {
        if (type is IProvideResolvedType provider)
        {
            return provider.ResolvedType?.InnerType();
        }

        return type;
    }

    public static bool TryGetValue(this GraphQLObjectValue source, string fieldName, out object value)
    {
        value = null!;

        if (source.Fields != null)
        {
            foreach (var field in source.Fields)
            {
                if (field.Name == fieldName)
                {
                    value = field.Value;
                    return true;
                }
            }
        }

        return false;
    }

    public static TimeSpan CacheDuration(this IResolveFieldContext context)
    {
        return CacheDirective.CacheDuration(context);
    }

    public static List<DomainId>? AsIds(this JsonValue value)
    {
        try
        {
            List<DomainId>? result = null;

            if (value.Value is JsonArray a)
            {
                foreach (var item in a)
                {
                    if (item.Value is string id)
                    {
                        result ??= [];
                        result.Add(DomainId.Create(id));
                    }
                }
            }

            return result;
        }
        catch
        {
            return null;
        }
    }

    public static bool HasOnlyIdField(this IResolveFieldContext context)
    {
        return context.FieldAst.SelectionSet?.Selections.TrueForAll(x => x is GraphQLField field && field.Name == "id") == true;
    }

    public static HashSet<string>? FieldNames(this IResolveFieldContext context)
    {
        if (!OptimizeFieldQueriesDirective.IsApplied(context))
        {
            return null;
        }

        return new FieldNameResolver(context.Document, context.Schema).Iterate(context.FieldAst, context.FieldDefinition.ResolvedType);
    }

    private sealed class FieldNameResolver
    {
        private readonly GraphQLDocument document;
        private readonly ISchema schema;
        private HashSet<string>? fieldNames = [];
        private IComplexGraphType? currentParent;

        public FieldNameResolver(GraphQLDocument document, ISchema schema)
        {
            this.document = document;
            this.schema = schema;
        }

        public HashSet<string>? Iterate(GraphQLField field, IGraphType? type)
        {
            currentParent = ResolveDataParent(type);

            IterateContent(field.SelectionSet);
            return fieldNames;
        }

        private void IterateContent(GraphQLSelectionSet? selection)
        {
            if (selection == null)
            {
                return;
            }

            foreach (var selectedField in selection.Selections)
            {
                switch (selectedField)
                {
                    case GraphQLField field when field.Name == "data":
                        IterateData(field.SelectionSet);
                        break;
                    case GraphQLField field when field.Name == "flatData":
                        IterateData(field.SelectionSet);
                        break;
                    case GraphQLField field when field.Name == "data__dynamic":
                        fieldNames = null;
                        return;
                    case GraphQLFragmentSpread spread:
                        var fragment = document.FindFragmentDefinition(spread.FragmentName.Name);

                        IterateContent(fragment?.SelectionSet);
                        break;

                    case GraphQLInlineFragment inline when inline.TypeCondition != null:
                        currentParent = ResolveDataParent(schema.AllTypes.FirstOrDefault(x => x.Name == inline.TypeCondition.Type.Name()));

                        IterateContent(inline.SelectionSet);
                        break;
                }
            }
        }

        private void IterateData(GraphQLSelectionSet? selection)
        {
            if (selection == null)
            {
                return;
            }

            foreach (var selectedField in selection.Selections)
            {
                switch (selectedField)
                {
                    case GraphQLField field when currentParent != null:
                        // The GraphQL field might be different from the schema name. Therefore we need the field type.
                        var fieldType = currentParent.Fields.Find(field.Name.StringValue);

                        if (fieldType != null)
                        {
                            // Resolve the schema name from the GraphQL field.
                            fieldNames?.Add(fieldType.SourceName());
                        }

                        break;
                    case GraphQLFragmentSpread spread:
                        var fragment = document.FindFragmentDefinition(spread.FragmentName.Name);

                        IterateData(fragment?.SelectionSet);
                        break;
                }
            }
        }

        private static IComplexGraphType? ResolveDataParent(IGraphType? type)
        {
            if (type?.InnerType() is not IComplexGraphType complexType)
            {
                return null;
            }

            // We need to resolve the schema names from the GraphQL field name and the flatData type has this information.
            if (complexType?.Fields.Find("flatData")?.ResolvedType?.InnerType() is not IComplexGraphType dataParent)
            {
                return null;
            }

            return dataParent;
        }
    }
}
