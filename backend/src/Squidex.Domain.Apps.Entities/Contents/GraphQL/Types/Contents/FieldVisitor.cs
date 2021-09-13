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
using GraphQL.Resolvers;
using GraphQL.Types;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Contents
{
    public delegate object ValueResolver(IJsonValue value, IResolveFieldContext fieldContext, GraphQLExecutionContext context);

    internal sealed class FieldVisitor : IFieldVisitor<(IGraphType?, IFieldResolver?, QueryArguments?), FieldInfo>
    {
        public static readonly IFieldResolver JsonNoop = CreateValueResolver((value, fieldContext, contex) => value);
        public static readonly IFieldResolver JsonPath = CreateValueResolver(ContentActions.Json.Resolver);

        private static readonly IFieldResolver JsonBoolean = CreateValueResolver((value, fieldContext, contex) =>
        {
            switch (value)
            {
                case JsonBoolean b:
                    return b.Value;
                default:
                    throw new NotSupportedException();
            }
        });

        private static readonly IFieldResolver JsonDateTime = CreateValueResolver((value, fieldContext, contex) =>
        {
            switch (value)
            {
                case JsonString n:
                    return n.Value;
                default:
                    throw new NotSupportedException();
            }
        });

        private static readonly IFieldResolver JsonNumber = CreateValueResolver((value, fieldContext, contex) =>
        {
            switch (value)
            {
                case JsonNumber n:
                    return n.Value;
                default:
                    throw new NotSupportedException();
            }
        });

        private static readonly IFieldResolver JsonString = CreateValueResolver((value, fieldContext, contex) =>
        {
            switch (value)
            {
                case JsonString s:
                    return s.Value;
                default:
                    throw new NotSupportedException();
            }
        });

        private static readonly IFieldResolver JsonStrings = CreateValueResolver((value, fieldContext, contex) =>
        {
            switch (value)
            {
                case JsonArray a:
                    return a.Select(x => x.ToString()).ToList();
                default:
                    throw new NotSupportedException();
            }
        });

        private static readonly IFieldResolver Assets = CreateValueResolver((value, fieldContext, context) =>
        {
            return context.GetReferencedAssetsAsync(value, fieldContext.CancellationToken);
        });

        private static readonly IFieldResolver References = CreateValueResolver((value, fieldContext, context) =>
        {
            return context.GetReferencedContentsAsync(value, fieldContext.CancellationToken);
        });

        private readonly Builder builder;

        public FieldVisitor(Builder builder)
        {
            this.builder = builder;
        }

        public (IGraphType?, IFieldResolver?, QueryArguments?) Visit(IArrayField field, FieldInfo args)
        {
            if (args.Fields.Count == 0)
            {
                return default;
            }

            var type = new NestedGraphType(builder, args);

            if (type.Fields.Count == 0)
            {
                return default;
            }

            return (new ListGraphType(new NonNullGraphType(type)), JsonNoop, null);
        }

        public (IGraphType?, IFieldResolver?, QueryArguments?) Visit(IField<AssetsFieldProperties> field, FieldInfo args)
        {
            return (builder.SharedTypes.AssetsList, Assets, null);
        }

        public (IGraphType?, IFieldResolver?, QueryArguments?) Visit(IField<BooleanFieldProperties> field, FieldInfo args)
        {
            return (AllTypes.Boolean, JsonBoolean, null);
        }

        public (IGraphType?, IFieldResolver?, QueryArguments?) Visit(IField<ComponentFieldProperties> field, FieldInfo args)
        {
            var type = ResolveComponent(args, field.Properties.SchemaIds);

            if (type == null)
            {
                return default;
            }

            return (type, JsonNoop, null);
        }

        public (IGraphType?, IFieldResolver?, QueryArguments?) Visit(IField<ComponentsFieldProperties> field, FieldInfo args)
        {
            var type = ResolveComponent(args, field.Properties.SchemaIds);

            if (type == null)
            {
                return default;
            }

            return (new ListGraphType(new NonNullGraphType(type)), JsonNoop, null);
        }

        public (IGraphType?, IFieldResolver?, QueryArguments?) Visit(IField<DateTimeFieldProperties> field, FieldInfo args)
        {
            return (AllTypes.DateTime, JsonDateTime, null);
        }

        public (IGraphType?, IFieldResolver?, QueryArguments?) Visit(IField<JsonFieldProperties> field, FieldInfo args)
        {
            return (AllTypes.Json, JsonPath, ContentActions.Json.Arguments);
        }

        public (IGraphType?, IFieldResolver?, QueryArguments?) Visit(IField<GeolocationFieldProperties> field, FieldInfo args)
        {
            return (AllTypes.Json, JsonPath, ContentActions.Json.Arguments);
        }

        public (IGraphType?, IFieldResolver?, QueryArguments?) Visit(IField<NumberFieldProperties> field, FieldInfo args)
        {
            return (AllTypes.Float, JsonNumber, null);
        }

        public (IGraphType?, IFieldResolver?, QueryArguments?) Visit(IField<StringFieldProperties> field, FieldInfo args)
        {
            return (AllTypes.String, JsonString, null);
        }

        public (IGraphType?, IFieldResolver?, QueryArguments?) Visit(IField<TagsFieldProperties> field, FieldInfo args)
        {
            return (AllTypes.Strings, JsonStrings, null);
        }

        public (IGraphType?, IFieldResolver?, QueryArguments?) Visit(IField<ReferencesFieldProperties> field, FieldInfo args)
        {
            var type = ResolveReferences(args, field.Properties.SchemaIds);

            if (type == null)
            {
                return default;
            }

            return (new ListGraphType(new NonNullGraphType(type)), References, null);
        }

        public (IGraphType?, IFieldResolver?, QueryArguments?) Visit(IField<UIFieldProperties> field, FieldInfo args)
        {
            return default;
        }

        private IGraphType? ResolveReferences(FieldInfo fieldInfo, ImmutableList<DomainId>? schemaIds)
        {
            IGraphType? contentType = null;

            if (schemaIds?.Count == 1)
            {
                contentType = builder.GetContentType(schemaIds[0]);
            }

            if (contentType == null)
            {
                var union = new ReferenceUnionGraphType(builder, fieldInfo, schemaIds);

                if (!union.HasType)
                {
                    return default;
                }

                contentType = union;
            }

            return contentType;
        }

        private IGraphType? ResolveComponent(FieldInfo fieldInfo, ImmutableList<DomainId>? schemaIds)
        {
            IGraphType? componentType = null;

            if (schemaIds?.Count == 1)
            {
                componentType = builder.GetComponentType(schemaIds[0]);
            }

            if (componentType == null)
            {
                var union = new ComponentUnionGraphType(builder, fieldInfo, schemaIds);

                if (!union.HasType)
                {
                    return default;
                }

                componentType = union;
            }

            return componentType;
        }

        private static IFieldResolver CreateValueResolver(ValueResolver valueResolver)
        {
            return Resolvers.Sync<IReadOnlyDictionary<string, IJsonValue>, object?>((source, fieldContext, context) =>
            {
                var key = fieldContext.FieldDefinition.SourceName();

                if (source.TryGetValue(key, out var value))
                {
                    if (value is JsonNull)
                    {
                        return null;
                    }

                    return valueResolver(value, fieldContext, context);
                }

                return null;
            });
        }
    }
}
