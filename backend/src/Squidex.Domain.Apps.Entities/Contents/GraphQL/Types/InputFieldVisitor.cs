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
    public sealed class InputFieldVisitor : IFieldVisitor<IGraphType?, InputFieldVisitor.Args>
    {
        private static readonly InputFieldVisitor Instance = new InputFieldVisitor();

        public readonly struct Args
        {
            public readonly IGraphModel Model;

            public readonly ISchemaEntity Schema;

            public readonly string SchemaField;

            public Args(IGraphModel model, ISchemaEntity schema, string fieldName)
            {
                Model = model;
                Schema = schema;
                SchemaField = fieldName;
            }
        }

        private InputFieldVisitor()
        {
        }

        public static IGraphType? Build(IField field, IGraphModel model, ISchemaEntity schema, string fieldName)
        {
            var args = new Args(model, schema, fieldName);

            return field.Accept(Instance, args);
        }

        public IGraphType? Visit(IArrayField field, Args args)
        {
            var schemaFieldType =
                new ListGraphType(
                    new NonNullGraphType(
                        new NestedInputGraphType(args.Model, args.Schema, field, args.SchemaField)));

            return schemaFieldType;
        }

        public IGraphType? Visit(IField<AssetsFieldProperties> field, Args args)
        {
            return AllTypes.References;
        }

        public IGraphType? Visit(IField<BooleanFieldProperties> field, Args args)
        {
            return AllTypes.Boolean;
        }

        public IGraphType? Visit(IField<DateTimeFieldProperties> field, Args args)
        {
            return AllTypes.Date;
        }

        public IGraphType? Visit(IField<GeolocationFieldProperties> field, Args args)
        {
            return GeolocationInputGraphType.Nullable;
        }

        public IGraphType? Visit(IField<JsonFieldProperties> field, Args args)
        {
            return AllTypes.Json;
        }

        public IGraphType? Visit(IField<NumberFieldProperties> field, Args args)
        {
            return AllTypes.Float;
        }

        public IGraphType? Visit(IField<ReferencesFieldProperties> field, Args args)
        {
            return AllTypes.Json;
        }

        public IGraphType? Visit(IField<StringFieldProperties> field, Args args)
        {
            return AllTypes.String;
        }

        public IGraphType? Visit(IField<TagsFieldProperties> field, Args args)
        {
            return AllTypes.Tags;
        }

        public IGraphType? Visit(IField<UIFieldProperties> field, Args args)
        {
            return null;
        }
    }
}
