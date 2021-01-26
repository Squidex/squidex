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
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Contents;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types
{
    public delegate object ValueResolver(IJsonValue value, IResolveFieldContext fieldContext, GraphQLExecutionContext context);

    internal sealed class GraphQLTypeVisitor : IFieldVisitor<(IGraphType?, ValueResolver?, QueryArguments?), GraphQLTypeVisitor.Args>
    {
        private static readonly ValueResolver NoopResolver = (value, fieldContext, contex) => value;

        private readonly Dictionary<DomainId, ContentGraphType> schemaTypes;
        private readonly IGraphModel model;

        public readonly struct Args
        {
            public readonly ISchemaEntity Schema;

            public readonly string SchemaField;

            public Args(ISchemaEntity schema, string fieldName)
            {
                Schema = schema;
                SchemaField = fieldName;
            }
        }

        public GraphQLTypeVisitor(Dictionary<DomainId, ContentGraphType> schemaTypes, IGraphModel model)
        {
            this.model = model;

            this.schemaTypes = schemaTypes;
        }

        public (IGraphType?, ValueResolver?, QueryArguments?) Visit(IArrayField field, Args args)
        {
            var schemaFieldType =
                new ListGraphType(
                    new NonNullGraphType(
                        new NestedGraphType(model, args.Schema, field, args.SchemaField)));

            return (schemaFieldType, NoopResolver, null);
        }

        public (IGraphType?, ValueResolver?, QueryArguments?) Visit(IField<AssetsFieldProperties> field, Args args)
        {
            return ResolveAssets();
        }

        public (IGraphType?, ValueResolver?, QueryArguments?) Visit(IField<BooleanFieldProperties> field, Args args)
        {
            return ResolveDefault(AllTypes.NoopBoolean);
        }

        public (IGraphType?, ValueResolver?, QueryArguments?) Visit(IField<DateTimeFieldProperties> field, Args args)
        {
            return ResolveDefault(AllTypes.NoopDate);
        }

        public (IGraphType?, ValueResolver?, QueryArguments?) Visit(IField<GeolocationFieldProperties> field, Args args)
        {
            return ResolveDefault(AllTypes.NoopGeolocation);
        }

        public (IGraphType?, ValueResolver?, QueryArguments?) Visit(IField<NumberFieldProperties> field, Args args)
        {
            return ResolveDefault(AllTypes.NoopFloat);
        }

        public (IGraphType?, ValueResolver?, QueryArguments?) Visit(IField<ReferencesFieldProperties> field, Args args)
        {
            return ResolveReferences(field, args);
        }

        public (IGraphType?, ValueResolver?, QueryArguments?) Visit(IField<StringFieldProperties> field, Args args)
        {
            return ResolveDefault(AllTypes.NoopString);
        }

        public (IGraphType?, ValueResolver?, QueryArguments?) Visit(IField<TagsFieldProperties> field, Args args)
        {
            return ResolveDefault(AllTypes.NoopTags);
        }

        public (IGraphType?, ValueResolver?, QueryArguments?) Visit(IField<UIFieldProperties> field, Args args)
        {
            return (null, null, null);
        }

        public (IGraphType?, ValueResolver?, QueryArguments?) Visit(IField<JsonFieldProperties> field, Args args)
        {
            return (AllTypes.NoopJson, ContentActions.Json.Resolver, ContentActions.Json.Arguments);
        }

        private static (IGraphType?, ValueResolver?, QueryArguments?) ResolveDefault(IGraphType type)
        {
            return (type, NoopResolver, null);
        }

        private (IGraphType?, ValueResolver?, QueryArguments?) ResolveAssets()
        {
            var resolver = new ValueResolver((value, _, context) =>
            {
                return context.GetReferencedAssetsAsync(value);
            });

            return (model.TypeFactory.AssetsList, resolver, null);
        }

        private (IGraphType?, ValueResolver?, QueryArguments?) ResolveReferences(IField<ReferencesFieldProperties> field, Args args)
        {
            IGraphType contentType = schemaTypes.GetOrDefault(field.Properties.SingleId());

            if (contentType == null)
            {
                var union = new ContentUnionGraphType(args.SchemaField, schemaTypes, field.Properties.SchemaIds);

                if (!union.PossibleTypes.Any())
                {
                    return (null, null, null);
                }

                contentType = union;
            }

            var resolver = new ValueResolver((value, _, context) =>
            {
                return context.GetReferencedContentsAsync(value);
            });

            var schemaFieldType = new ListGraphType(new NonNullGraphType(contentType));

            return (schemaFieldType, resolver, null);
        }
    }
}
