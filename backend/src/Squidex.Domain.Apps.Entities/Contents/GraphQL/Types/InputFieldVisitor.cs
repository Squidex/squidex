// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL.Types;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Utils;
using Squidex.Domain.Apps.Entities.Schemas;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types
{
    public sealed class InputFieldVisitor : IFieldVisitor<IGraphType?>
    {
        private readonly ISchemaEntity schema;
        private readonly IGraphModel model;
        private readonly string fieldName;

        public InputFieldVisitor(ISchemaEntity schema, IGraphModel model, string fieldName)
        {
            this.model = model;
            this.schema = schema;
            this.fieldName = fieldName;
        }

        public IGraphType? Visit(IArrayField field)
        {
            var schemaFieldType = new ListGraphType(new NonNullGraphType(new NestedInputGraphType(model, schema, field, fieldName)));

            return schemaFieldType;
        }

        public IGraphType? Visit(IField<AssetsFieldProperties> field)
        {
            return AllTypes.References;
        }

        public IGraphType? Visit(IField<BooleanFieldProperties> field)
        {
            return AllTypes.Boolean;
        }

        public IGraphType? Visit(IField<DateTimeFieldProperties> field)
        {
            return AllTypes.Date;
        }

        public IGraphType? Visit(IField<GeolocationFieldProperties> field)
        {
            return GeolocationInputGraphType.Nullable;
        }

        public IGraphType? Visit(IField<JsonFieldProperties> field)
        {
            return AllTypes.Json;
        }

        public IGraphType? Visit(IField<NumberFieldProperties> field)
        {
            return AllTypes.Float;
        }

        public IGraphType? Visit(IField<ReferencesFieldProperties> field)
        {
            return AllTypes.Json;
        }

        public IGraphType? Visit(IField<StringFieldProperties> field)
        {
            return AllTypes.String;
        }

        public IGraphType? Visit(IField<TagsFieldProperties> field)
        {
            return AllTypes.Tags;
        }

        public IGraphType? Visit(IField<UIFieldProperties> field)
        {
            return null;
        }
    }
}
