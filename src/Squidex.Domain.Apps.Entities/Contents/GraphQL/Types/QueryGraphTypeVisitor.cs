// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using GraphQL.Types;
using Newtonsoft.Json.Linq;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Schemas;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types
{
    public delegate object ValueResolver(JToken value, ResolveFieldContext context);

    public sealed class QueryGraphTypeVisitor : IFieldVisitor<(IGraphType ResolveType, ValueResolver Resolver)>
    {
        private static readonly ValueResolver NoopResolver = new ValueResolver((value, c) => value);
        private readonly ISchemaEntity schema;
        private readonly Func<Guid, IGraphType> schemaResolver;
        private readonly IGraphModel model;
        private readonly IGraphType assetListType;

        public QueryGraphTypeVisitor(ISchemaEntity schema, Func<Guid, IGraphType> schemaResolver, IGraphModel model, IGraphType assetListType)
        {
            this.model = model;
            this.assetListType = assetListType;
            this.schema = schema;
            this.schemaResolver = schemaResolver;
        }

        public (IGraphType ResolveType, ValueResolver Resolver) Visit(IArrayField field)
        {
            return ResolveNested(field);
        }

        public (IGraphType ResolveType, ValueResolver Resolver) Visit(IField<AssetsFieldProperties> field)
        {
            return ResolveAssets();
        }

        public (IGraphType ResolveType, ValueResolver Resolver) Visit(IField<BooleanFieldProperties> field)
        {
            return ResolveDefault(AllTypes.NoopBoolean);
        }

        public (IGraphType ResolveType, ValueResolver Resolver) Visit(IField<DateTimeFieldProperties> field)
        {
            return ResolveDefault(AllTypes.NoopDate);
        }

        public (IGraphType ResolveType, ValueResolver Resolver) Visit(IField<GeolocationFieldProperties> field)
        {
            return ResolveDefault(AllTypes.NoopGeolocation);
        }

        public (IGraphType ResolveType, ValueResolver Resolver) Visit(IField<JsonFieldProperties> field)
        {
            return ResolveDefault(AllTypes.NoopJson);
        }

        public (IGraphType ResolveType, ValueResolver Resolver) Visit(IField<NumberFieldProperties> field)
        {
            return ResolveDefault(AllTypes.NoopFloat);
        }

        public (IGraphType ResolveType, ValueResolver Resolver) Visit(IField<ReferencesFieldProperties> field)
        {
            return ResolveReferences(field);
        }

        public (IGraphType ResolveType, ValueResolver Resolver) Visit(IField<StringFieldProperties> field)
        {
            return ResolveDefault(AllTypes.NoopString);
        }

        public (IGraphType ResolveType, ValueResolver Resolver) Visit(IField<TagsFieldProperties> field)
        {
            return ResolveDefault(AllTypes.NoopTags);
        }

        private static (IGraphType ResolveType, ValueResolver Resolver) ResolveDefault(IGraphType type)
        {
            return (type, NoopResolver);
        }

        private (IGraphType ResolveType, ValueResolver Resolver) ResolveNested(IArrayField field)
        {
            var schemaFieldType = new ListGraphType(new NonNullGraphType(new NestedGraphType(model, schema, field)));

            return (schemaFieldType, NoopResolver);
        }

        private (IGraphType ResolveType, ValueResolver Resolver) ResolveAssets()
        {
            var resolver = new ValueResolver((value, c) =>
            {
                var context = (GraphQLExecutionContext)c.UserContext;

                return context.GetReferencedAssetsAsync(value);
            });

            return (assetListType, resolver);
        }

        private (IGraphType ResolveType, ValueResolver Resolver) ResolveReferences(IField field)
        {
            var schemaId = ((ReferencesFieldProperties)field.RawProperties).SchemaId;

            var contentType = schemaResolver(schemaId);

            if (contentType == null)
            {
                return (null, null);
            }

            var resolver = new ValueResolver((value, c) =>
            {
                var context = (GraphQLExecutionContext)c.UserContext;

                return context.GetReferencedContentsAsync(schemaId, value);
            });

            var schemaFieldType = new ListGraphType(new NonNullGraphType(contentType));

            return (schemaFieldType, resolver);
        }
    }
}
