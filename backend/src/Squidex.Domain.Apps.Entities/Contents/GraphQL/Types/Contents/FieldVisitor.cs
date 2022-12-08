// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL;
using GraphQL.Resolvers;
using GraphQL.Types;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;
using Squidex.Infrastructure.Json.Objects;

#pragma warning disable MA0048 // File name must match type name

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Contents;

public delegate T ValueResolver<out T>(JsonValue value, IResolveFieldContext fieldContext, GraphQLExecutionContext context);

public delegate Task<T> AsyncValueResolver<T>(JsonValue value, IResolveFieldContext fieldContext, GraphQLExecutionContext context);

internal sealed class FieldVisitor : IFieldVisitor<FieldGraphSchema, FieldInfo>
{
    public static readonly IFieldResolver JsonNoop = CreateValueResolver((value, fieldContext, contex) => value.Value);
    public static readonly IFieldResolver JsonPath = CreateValueResolver(ContentActions.Json.Resolver);

    private static readonly IFieldResolver JsonBoolean = CreateValueResolver((value, fieldContext, contex) =>
    {
        switch (value.Value)
        {
            case bool b:
                return b;
            default:
                ThrowHelper.NotSupportedException();
                return default!;
        }
    });

    private static readonly IFieldResolver JsonComponents = CreateValueResolver((value, fieldContext, contex) =>
    {
        switch (value.Value)
        {
            case JsonArray a:
                return a.Select(x => x.AsObject).ToList();
            default:
                ThrowHelper.NotSupportedException();
                return default!;
        }
    });

    private static readonly IFieldResolver JsonDateTime = CreateValueResolver((value, fieldContext, contex) =>
    {
        switch (value.Value)
        {
            case string s:
                return s;
            default:
                ThrowHelper.NotSupportedException();
                return default!;
        }
    });

    private static readonly IFieldResolver JsonNumber = CreateValueResolver((value, fieldContext, contex) =>
    {
        switch (value.Value)
        {
            case double n:
                return n;
            default:
                ThrowHelper.NotSupportedException();
                return default!;
        }
    });

    private static readonly IFieldResolver JsonString = CreateValueResolver((value, fieldContext, contex) =>
    {
        switch (value.Value)
        {
            case string s:
                return s;
            default:
                ThrowHelper.NotSupportedException();
                return default!;
        }
    });

    private static readonly IFieldResolver JsonStrings = CreateValueResolver((value, fieldContext, contex) =>
    {
        switch (value.Value)
        {
            case JsonArray a:
                return a.Select(x => x.ToString()).ToList();
            default:
                ThrowHelper.NotSupportedException();
                return default!;
        }
    });

    private static readonly IFieldResolver Assets = CreateAsyncValueResolver((value, fieldContext, context) =>
    {
        var cacheDuration = fieldContext.CacheDuration();

        return context.GetReferencedAssetsAsync(value, cacheDuration, fieldContext.CancellationToken);
    });

    private static readonly IFieldResolver References = CreateAsyncValueResolver((value, fieldContext, context) =>
    {
        var cacheDuration = fieldContext.CacheDuration();

        return context.GetReferencedContentsAsync(value, cacheDuration, fieldContext.CancellationToken);
    });

    private readonly Builder builder;

    public FieldVisitor(Builder builder)
    {
        this.builder = builder;
    }

    public FieldGraphSchema Visit(IArrayField field, FieldInfo args)
    {
        if (args.Fields.Count == 0)
        {
            return default;
        }

        var type = builder.GetNested(args);

        if (type.Fields.Count == 0)
        {
            return default;
        }

        return new (new ListGraphType(new NonNullGraphType(type)), JsonComponents, null);
    }

    public FieldGraphSchema Visit(IField<AssetsFieldProperties> field, FieldInfo args)
    {
        return new (SharedTypes.AssetsList, Assets, null);
    }

    public FieldGraphSchema Visit(IField<BooleanFieldProperties> field, FieldInfo args)
    {
        return new (Scalars.Boolean, JsonBoolean, null);
    }

    public FieldGraphSchema Visit(IField<ComponentFieldProperties> field, FieldInfo args)
    {
        var type = ResolveComponent(args, field.Properties.SchemaIds);

        if (type == null)
        {
            return default;
        }

        return new (type, JsonNoop, null);
    }

    public FieldGraphSchema Visit(IField<ComponentsFieldProperties> field, FieldInfo args)
    {
        var type = ResolveComponent(args, field.Properties.SchemaIds);

        if (type == null)
        {
            return default;
        }

        return new (new ListGraphType(new NonNullGraphType(type)), JsonComponents, null);
    }

    public FieldGraphSchema Visit(IField<DateTimeFieldProperties> field, FieldInfo args)
    {
        return new (Scalars.DateTime, JsonDateTime, null);
    }

    public FieldGraphSchema Visit(IField<JsonFieldProperties> field, FieldInfo args)
    {
        var schema = builder.GetDynamicTypes(field.Properties.GraphQLSchema);

        if (schema.Length > 0)
        {
            return new (schema[0], JsonNoop, null);
        }

        return new (Scalars.Json, JsonPath, ContentActions.Json.Arguments);
    }

    public FieldGraphSchema Visit(IField<GeolocationFieldProperties> field, FieldInfo args)
    {
        return new (Scalars.Json, JsonPath, ContentActions.Json.Arguments);
    }

    public FieldGraphSchema Visit(IField<NumberFieldProperties> field, FieldInfo args)
    {
        return new (Scalars.Float, JsonNumber, null);
    }

    public FieldGraphSchema Visit(IField<StringFieldProperties> field, FieldInfo args)
    {
        var type = Scalars.String;

        if (field.Properties.IsEmbeddable)
        {
            type = builder.GetEmbeddableString(args, field.Properties);
        }
        else if (field.Properties?.AllowedValues?.Count > 0 && field.Properties.CreateEnum)
        {
            var @enum = builder.GetEnumeration(args.EmbeddedEnumType, field.Properties.AllowedValues);

            if (@enum != null)
            {
                type = @enum;
            }
        }

        return new (type, JsonString, null);
    }

    public FieldGraphSchema Visit(IField<TagsFieldProperties> field, FieldInfo args)
    {
        var type = Scalars.Strings;

        if (field.Properties?.AllowedValues?.Count > 0 && field.Properties.CreateEnum)
        {
            var @enum = builder.GetEnumeration(args.EmbeddedEnumType, field.Properties.AllowedValues);

            if (@enum != null)
            {
                type = new ListGraphType(new NonNullGraphType(@enum));
            }
        }

        return new (type, JsonStrings, null);
    }

    public FieldGraphSchema Visit(IField<ReferencesFieldProperties> field, FieldInfo args)
    {
        var type = ResolveReferences(args, field.Properties.SchemaIds);

        if (type == null)
        {
            return default;
        }

        return new (new ListGraphType(new NonNullGraphType(type)), References, null);
    }

    public FieldGraphSchema Visit(IField<UIFieldProperties> field, FieldInfo args)
    {
        return default;
    }

    private IGraphType? ResolveReferences(FieldInfo fieldInfo, ReadonlyList<DomainId>? schemaIds)
    {
        IGraphType? contentType = null;

        if (schemaIds?.Count == 1)
        {
            contentType = builder.GetContentType(schemaIds[0]);
        }

        if (contentType == null)
        {
            var union = builder.GetReferenceUnion(fieldInfo, schemaIds);

            if (!union.HasType)
            {
                return null;
            }

            contentType = union;
        }

        return contentType;
    }

    private IGraphType? ResolveComponent(FieldInfo fieldInfo, ReadonlyList<DomainId>? schemaIds)
    {
        IGraphType? componentType = null;

        if (schemaIds?.Count == 1)
        {
            componentType = builder.GetComponentType(schemaIds[0]);
        }

        if (componentType == null)
        {
            var union = builder.GetComponentUnion(fieldInfo, schemaIds);

            if (!union.HasType)
            {
                return null;
            }

            componentType = union;
        }

        return componentType;
    }

    private static IFieldResolver CreateValueResolver<T>(ValueResolver<T> valueResolver)
    {
        return Resolvers.Sync<IReadOnlyDictionary<string, JsonValue>, object?>((source, fieldContext, context) =>
        {
            var key = fieldContext.FieldDefinition.SourceName();

            if (source.TryGetValue(key, out var value))
            {
                if (value == JsonValue.Null)
                {
                    return null;
                }

                return valueResolver(value, fieldContext, context);
            }

            return null;
        });
    }

    private static IFieldResolver CreateAsyncValueResolver<T>(AsyncValueResolver<T> valueResolver)
    {
        return Resolvers.Async<IReadOnlyDictionary<string, JsonValue>, object?>(async (source, fieldContext, context) =>
        {
            var key = fieldContext.FieldDefinition.SourceName();

            if (source.TryGetValue(key, out var value))
            {
                if (value == JsonValue.Null)
                {
                    return null;
                }

                return await valueResolver(value, fieldContext, context);
            }

            return null;
        });
    }
}
