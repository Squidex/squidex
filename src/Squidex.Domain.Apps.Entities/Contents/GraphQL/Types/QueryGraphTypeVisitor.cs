// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using GraphQL.Resolvers;
using GraphQL.Types;
using Newtonsoft.Json.Linq;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types
{
    public sealed class QueryGraphTypeVisitor : IFieldVisitor<(IGraphType ResolveType, IFieldResolver Resolver)>
    {
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

        public (IGraphType ResolveType, IFieldResolver Resolver) Visit(IArrayField field)
        {
            return ResolveNested(field);
        }

        public (IGraphType ResolveType, IFieldResolver Resolver) Visit(IField<AssetsFieldProperties> field)
        {
            return ResolveAssets(assetListType);
        }

        public (IGraphType ResolveType, IFieldResolver Resolver) Visit(IField<BooleanFieldProperties> field)
        {
            return ResolveDefault(AllTypes.NoopBoolean);
        }

        public (IGraphType ResolveType, IFieldResolver Resolver) Visit(IField<DateTimeFieldProperties> field)
        {
            return ResolveDefault(AllTypes.NoopDate);
        }

        public (IGraphType ResolveType, IFieldResolver Resolver) Visit(IField<GeolocationFieldProperties> field)
        {
            return ResolveDefault(AllTypes.NoopGeolocation);
        }

        public (IGraphType ResolveType, IFieldResolver Resolver) Visit(IField<JsonFieldProperties> field)
        {
            return ResolveDefault(AllTypes.NoopJson);
        }

        public (IGraphType ResolveType, IFieldResolver Resolver) Visit(IField<NumberFieldProperties> field)
        {
            return ResolveDefault(AllTypes.NoopFloat);
        }

        public (IGraphType ResolveType, IFieldResolver Resolver) Visit(IField<ReferencesFieldProperties> field)
        {
            return ResolveReferences(field);
        }

        public (IGraphType ResolveType, IFieldResolver Resolver) Visit(IField<StringFieldProperties> field)
        {
            return ResolveDefault(AllTypes.NoopString);
        }

        public (IGraphType ResolveType, IFieldResolver Resolver) Visit(IField<TagsFieldProperties> field)
        {
            return ResolveDefault(AllTypes.NoopTags);
        }

        private static (IGraphType ResolveType, IFieldResolver Resolver) ResolveDefault(IGraphType type)
        {
            return (type, new FuncFieldResolver<IReadOnlyDictionary<string, JToken>, object>(c => c.Source.GetOrDefault(c.FieldName)));
        }

        private static ValueTuple<IGraphType, IFieldResolver> ResolveAssets(IGraphType assetListType)
        {
            var resolver = new FuncFieldResolver<IReadOnlyDictionary<string, JToken>, object>(c =>
            {
                var context = (GraphQLExecutionContext)c.UserContext;
                var contentIds = c.Source.GetOrDefault(c.FieldName);

                return context.GetReferencedAssetsAsync(contentIds);
            });

            return (assetListType, resolver);
        }

        private ValueTuple<IGraphType, IFieldResolver> ResolveNested(IArrayField field)
        {
            var resolver = new FuncFieldResolver<IReadOnlyDictionary<string, JToken>, object>(c =>
            {
                return c.Source.GetOrDefault(c.FieldName);
            });

            var schemaFieldType = new ListGraphType(new NonNullGraphType(new NestedObjectGraphType(model, schema, field)));

            return (schemaFieldType, resolver);
        }

        private ValueTuple<IGraphType, IFieldResolver> ResolveReferences(IField field)
        {
            var schemaId = ((ReferencesFieldProperties)field.RawProperties).SchemaId;

            var contentType = schemaResolver(schemaId);

            if (contentType == null)
            {
                return (null, null);
            }

            var resolver = new FuncFieldResolver<IReadOnlyDictionary<string, JToken>, object>(c =>
            {
                var context = (GraphQLExecutionContext)c.UserContext;
                var contentIds = c.Source.GetOrDefault(c.FieldName);

                return context.GetReferencedContentsAsync(schemaId, contentIds);
            });

            var schemaFieldType = new ListGraphType(new NonNullGraphType(contentType));

            return (schemaFieldType, resolver);
        }
    }
}
