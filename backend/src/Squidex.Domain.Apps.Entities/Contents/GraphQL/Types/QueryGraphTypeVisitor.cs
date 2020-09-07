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
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types
{
    public delegate object ValueResolver(IJsonValue value, IResolveFieldContext context);

    public sealed class QueryGraphTypeVisitor : IFieldVisitor<(IGraphType?, ValueResolver?, QueryArguments?)>
    {
        private static readonly ValueResolver NoopResolver = (value, c) => value;
        private readonly Dictionary<DomainId, ContentGraphType> schemaTypes;
        private readonly ISchemaEntity schema;
        private readonly IGraphModel model;
        private readonly IGraphType assetListType;
        private readonly string fieldName;

        public QueryGraphTypeVisitor(ISchemaEntity schema,
            Dictionary<DomainId, ContentGraphType> schemaTypes,
            IGraphModel model,
            IGraphType assetListType,
            string fieldName)
        {
            this.model = model;
            this.assetListType = assetListType;
            this.schema = schema;
            this.schemaTypes = schemaTypes;
            this.fieldName = fieldName;
        }

        public (IGraphType?, ValueResolver?, QueryArguments?) Visit(IArrayField field)
        {
            var schemaFieldType = new ListGraphType(new NonNullGraphType(new NestedGraphType(model, schema, field, fieldName)));

            return (schemaFieldType, NoopResolver, null);
        }

        public (IGraphType?, ValueResolver?, QueryArguments?) Visit(IField<AssetsFieldProperties> field)
        {
            return ResolveAssets();
        }

        public (IGraphType?, ValueResolver?, QueryArguments?) Visit(IField<BooleanFieldProperties> field)
        {
            return ResolveDefault(AllTypes.NoopBoolean);
        }

        public (IGraphType?, ValueResolver?, QueryArguments?) Visit(IField<DateTimeFieldProperties> field)
        {
            return ResolveDefault(AllTypes.NoopDate);
        }

        public (IGraphType?, ValueResolver?, QueryArguments?) Visit(IField<GeolocationFieldProperties> field)
        {
            return ResolveDefault(AllTypes.NoopGeolocation);
        }

        public (IGraphType?, ValueResolver?, QueryArguments?) Visit(IField<NumberFieldProperties> field)
        {
            return ResolveDefault(AllTypes.NoopFloat);
        }

        public (IGraphType?, ValueResolver?, QueryArguments?) Visit(IField<ReferencesFieldProperties> field)
        {
            return ResolveReferences(field);
        }

        public (IGraphType?, ValueResolver?, QueryArguments?) Visit(IField<StringFieldProperties> field)
        {
            return ResolveDefault(AllTypes.NoopString);
        }

        public (IGraphType?, ValueResolver?, QueryArguments?) Visit(IField<TagsFieldProperties> field)
        {
            return ResolveDefault(AllTypes.NoopTags);
        }

        public (IGraphType?, ValueResolver?, QueryArguments?) Visit(IField<UIFieldProperties> field)
        {
            return (null, null, null);
        }

        public (IGraphType?, ValueResolver?, QueryArguments?) Visit(IField<JsonFieldProperties> field)
        {
            return (AllTypes.NoopJson, ContentActions.Json.Resolver, ContentActions.Json.Arguments);
        }

        private static (IGraphType?, ValueResolver?, QueryArguments?) ResolveDefault(IGraphType type)
        {
            return (type, NoopResolver, null);
        }

        private (IGraphType?, ValueResolver?, QueryArguments?) ResolveAssets()
        {
            var resolver = new ValueResolver((value, c) =>
            {
                var context = (GraphQLExecutionContext)c.UserContext;

                return context.GetReferencedAssetsAsync(value);
            });

            return (assetListType, resolver, null);
        }

        private (IGraphType?, ValueResolver?, QueryArguments?) ResolveReferences(IField<ReferencesFieldProperties> field)
        {
            IGraphType contentType = schemaTypes.GetOrDefault(field.Properties.SingleId());

            if (contentType == null)
            {
                var union = new ContentUnionGraphType(fieldName, schemaTypes, field.Properties.SchemaIds);

                if (!union.PossibleTypes.Any())
                {
                    return (null, null, null);
                }

                contentType = union;
            }

            var resolver = new ValueResolver((value, c) =>
            {
                var context = (GraphQLExecutionContext)c.UserContext;

                return context.GetReferencedContentsAsync(value);
            });

            var schemaFieldType = new ListGraphType(new NonNullGraphType(contentType));

            return (schemaFieldType, resolver, null);
        }
    }
}
