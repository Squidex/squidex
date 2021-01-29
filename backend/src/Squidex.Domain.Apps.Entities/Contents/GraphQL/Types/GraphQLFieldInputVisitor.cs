// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL.Types;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Contents;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types
{
    public sealed class GraphQLFieldInputVisitor : IFieldVisitor<IGraphType?, FieldInfo>
    {
        private readonly GraphQLModel model;

        public GraphQLFieldInputVisitor(GraphQLModel model)
        {
            this.model = model;
        }

        public IGraphType? Visit(IArrayField field, FieldInfo args)
        {
            var schemaFieldType =
                new ListGraphType(
                    new NonNullGraphType(
                        new NestedInputGraphType(model, args)));

            return schemaFieldType;
        }

        public IGraphType? Visit(IField<AssetsFieldProperties> field, FieldInfo args)
        {
            return AllTypes.References;
        }

        public IGraphType? Visit(IField<BooleanFieldProperties> field, FieldInfo args)
        {
            return AllTypes.Boolean;
        }

        public IGraphType? Visit(IField<DateTimeFieldProperties> field, FieldInfo args)
        {
            return AllTypes.Date;
        }

        public IGraphType? Visit(IField<GeolocationFieldProperties> field, FieldInfo args)
        {
            return AllTypes.Json;
        }

        public IGraphType? Visit(IField<JsonFieldProperties> field, FieldInfo args)
        {
            return AllTypes.Json;
        }

        public IGraphType? Visit(IField<NumberFieldProperties> field, FieldInfo args)
        {
            return AllTypes.Float;
        }

        public IGraphType? Visit(IField<ReferencesFieldProperties> field, FieldInfo args)
        {
            return AllTypes.Json;
        }

        public IGraphType? Visit(IField<StringFieldProperties> field, FieldInfo args)
        {
            return AllTypes.String;
        }

        public IGraphType? Visit(IField<TagsFieldProperties> field, FieldInfo args)
        {
            return AllTypes.Tags;
        }

        public IGraphType? Visit(IField<UIFieldProperties> field, FieldInfo args)
        {
            return null;
        }
    }
}
