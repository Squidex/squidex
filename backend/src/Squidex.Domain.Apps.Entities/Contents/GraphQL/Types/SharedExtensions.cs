// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Google.Protobuf.WellKnownTypes;
using GraphQL;
using GraphQL.Types;
using GraphQL.Utilities;
using GraphQLParser;
using GraphQLParser.AST;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Contents;
using Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Directives;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.ObjectPool;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types;

public static class SharedExtensions
{
    private static readonly IReadOnlyList<IEnrichedAssetEntity> EmptyAssets = new List<IEnrichedAssetEntity>();
    private static readonly IReadOnlyList<IEnrichedContentEntity> EmptyContents = new List<IEnrichedContentEntity>();

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

    internal static FieldType WithSourceName(this FieldType field, string value)
    {
        return field.WithMetadata(nameof(SourceName), value);
    }

    internal static FieldType WithSourceName(this FieldType field, FieldInfo value)
    {
        return field.WithMetadata(nameof(SourceName), value.Field.Name);
    }

    internal static string SourceName(this FieldType field)
    {
        return field.GetMetadata<string>(nameof(SourceName))!;
    }

    internal static FieldType WithSchemaId(this FieldType field, SchemaInfo value)
    {
        return field.WithMetadata(nameof(SchemaId), value.Schema.Id.ToString());
    }

    internal static string SchemaId(this FieldType field)
    {
        return field.GetMetadata<string>(nameof(SchemaId))!;
    }

    internal static FieldType WithSchemaNamedId(this FieldType field, SchemaInfo value)
    {
        return field.WithMetadata(nameof(SchemaNamedId), value.Schema.NamedId());
    }

    internal static NamedId<DomainId> SchemaNamedId(this FieldType field)
    {
        return field.GetMetadata<NamedId<DomainId>>(nameof(SchemaNamedId))!;
    }

    public static IGraphType? InnerType(this IGraphType type)
    {
        if (type is IProvideResolvedType provider)
        {
            return provider.ResolvedType?.InnerType();
        }

        return type;
    }

    public static TimeSpan CacheDuration(this IResolveFieldContext context)
    {
        return CacheDirective.CacheDuration(context);
    }

    public static Task<IReadOnlyList<IEnrichedAssetEntity>> ResolveAssetsAsync(this IResolveFieldContext fieldContext, List<DomainId>? ids)
    {
        if (ids == null || ids.Count == 0)
        {
            return Task.FromResult(EmptyAssets);
        }

        var context = (GraphQLExecutionContext)fieldContext.UserContext;

        if (fieldContext.HasOnlyIdField())
        {
            var contents = ids.Select(x => (IEnrichedAssetEntity)new AssetEntity
            {
                Id = x,
                Version = 0
            }).ToList();

            return Task.FromResult<IReadOnlyList<IEnrichedAssetEntity>>(contents);
        }

        return context.GetAssetsAsync(ids,
            fieldContext.CacheDuration(),
            fieldContext.CancellationToken);
    }

    public static Task<IReadOnlyList<IEnrichedContentEntity>> ResolveContentsAsync(this IResolveFieldContext fieldContext, List<DomainId>? ids)
    {
        if (ids == null || ids.Count == 0)
        {
            return Task.FromResult(EmptyContents);
        }

        var context = (GraphQLExecutionContext)fieldContext.UserContext;

        if (fieldContext.HasOnlyIdField())
        {
            var schemaId = GetSchemaId(fieldContext);

            var contents = ids.Select(id => (IEnrichedContentEntity)new ContentEntity
            {
                Id = id,
                SchemaId = schemaId
            }).ToList();

            return Task.FromResult<IReadOnlyList<IEnrichedContentEntity>>(contents);
        }

        return context.GetContentsAsync(ids,
            fieldContext.FieldNamesWhenToggled(),
            fieldContext.CacheDuration(),
            fieldContext.CancellationToken);

        static NamedId<DomainId> GetSchemaId(IResolveFieldContext fieldContext)
        {
            var schemaId = default(DomainId);
            switch (fieldContext.FieldDefinition.ResolvedType?.InnerType())
            {
                case ContentGraphType content:
                    schemaId = content.SchemaId;
                    break;
                case ContentUnionGraphType union:
                    schemaId = union.SchemaTypes.Keys.First();
                    break;
            }

            return NamedId.Of(schemaId, "Unresolved");
        }
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
                        result ??= new List<DomainId>();
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

    public static HashSet<string>? FieldNamesWhenToggled(this IResolveFieldContext context)
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
        private HashSet<string>? fieldNames = new HashSet<string>();
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
