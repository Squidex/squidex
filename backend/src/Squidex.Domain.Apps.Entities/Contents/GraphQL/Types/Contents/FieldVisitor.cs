// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using GraphQL;
using GraphQL.Resolvers;
using GraphQL.Types;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Contents
{
    public delegate object ValueResolver(IJsonValue value, IResolveFieldContext fieldContext, GraphQLExecutionContext context);

    internal sealed class FieldVisitor : IFieldVisitor<(IGraphType?, IFieldResolver?, QueryArguments?), FieldInfo>
    {
        private static readonly IFieldResolver Noop = CreateValueResolver((value, fieldContext, contex) => value);
        private static readonly IFieldResolver Json = CreateValueResolver(ContentActions.Json.Resolver);

        private static readonly IFieldResolver Assets = CreateValueResolver((value, _, context) =>
        {
            return context.GetReferencedAssetsAsync(value);
        });

        private static readonly IFieldResolver References = CreateValueResolver((value, _, context) =>
        {
            return context.GetReferencedContentsAsync(value);
        });

        private readonly Builder builder;

        public FieldVisitor(Builder builder)
        {
            this.builder = builder;
        }

        public (IGraphType?, IFieldResolver?, QueryArguments?) Visit(IArrayField field, FieldInfo args)
        {
            var schemaFieldType =
                new ListGraphType(
                    new NonNullGraphType(
                        new NestedGraphType(builder, args)));

            return (schemaFieldType, Noop, null);
        }

        public (IGraphType?, IFieldResolver?, QueryArguments?) Visit(IField<AssetsFieldProperties> field, FieldInfo args)
        {
            return (builder.SharedTypes.AssetsList, Assets, null);
        }

        public (IGraphType?, IFieldResolver?, QueryArguments?) Visit(IField<BooleanFieldProperties> field, FieldInfo args)
        {
            return (AllTypes.Boolean, Noop, null);
        }

        public (IGraphType?, IFieldResolver?, QueryArguments?) Visit(IField<DateTimeFieldProperties> field, FieldInfo args)
        {
            return (AllTypes.Date, Noop, null);
        }

        public (IGraphType?, IFieldResolver?, QueryArguments?) Visit(IField<JsonFieldProperties> field, FieldInfo args)
        {
            return (AllTypes.Json, Json, ContentActions.Json.Arguments);
        }

        public (IGraphType?, IFieldResolver?, QueryArguments?) Visit(IField<GeolocationFieldProperties> field, FieldInfo args)
        {
            return (AllTypes.Json, Noop, null);
        }

        public (IGraphType?, IFieldResolver?, QueryArguments?) Visit(IField<NumberFieldProperties> field, FieldInfo args)
        {
            return (AllTypes.Float, Noop, null);
        }

        public (IGraphType?, IFieldResolver?, QueryArguments?) Visit(IField<ReferencesFieldProperties> field, FieldInfo args)
        {
            return ResolveReferences(field, args);
        }

        public (IGraphType?, IFieldResolver?, QueryArguments?) Visit(IField<StringFieldProperties> field, FieldInfo args)
        {
            return (AllTypes.String, Noop, null);
        }

        public (IGraphType?, IFieldResolver?, QueryArguments?) Visit(IField<TagsFieldProperties> field, FieldInfo args)
        {
            return (AllTypes.Strings, Noop, null);
        }

        public (IGraphType?, IFieldResolver?, QueryArguments?) Visit(IField<UIFieldProperties> field, FieldInfo args)
        {
            return (null, null, null);
        }

        private (IGraphType?, IFieldResolver?, QueryArguments?) ResolveReferences(IField<ReferencesFieldProperties> field, FieldInfo args)
        {
            IGraphType? contentType = builder.GetContentType(field.Properties.SingleId());

            if (contentType == null)
            {
                var union = new ContentUnionGraphType(builder, args, field.Properties);

                if (!union.PossibleTypes.Any())
                {
                    return (null, null, null);
                }

                contentType = union;
            }

            var schemaFieldType = new ListGraphType(new NonNullGraphType(contentType));

            return (schemaFieldType, References, null);
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
