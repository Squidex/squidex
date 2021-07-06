// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Schemas.Commands;
using Squidex.Text;

namespace Squidex.Domain.Apps.Entities.Apps.Templates.Builders
{
    public class ArrayFieldBuilder : FieldBuilder<ArrayFieldBuilder>
    {
        private UpsertSchemaField TypedField
        {
            get => (UpsertSchemaField)Field;
        }

        public ArrayFieldBuilder(UpsertSchemaField field, CreateSchema schema)
            : base(field, schema)
        {
        }

        public ArrayFieldBuilder AddAssets(string name, Action<AssetFieldBuilder> configure)
        {
            var field = AddField<AssetsFieldProperties>(name);

            configure(new AssetFieldBuilder(field, Schema));

            return this;
        }

        public ArrayFieldBuilder AddBoolean(string name, Action<BooleanFieldBuilder> configure)
        {
            var field = AddField<BooleanFieldProperties>(name);

            configure(new BooleanFieldBuilder(field, Schema));

            return this;
        }

        public ArrayFieldBuilder AddDateTime(string name, Action<DateTimeFieldBuilder> configure)
        {
            var field = AddField<DateTimeFieldProperties>(name);

            configure(new DateTimeFieldBuilder(field, Schema));

            return this;
        }

        public ArrayFieldBuilder AddJson(string name, Action<JsonFieldBuilder> configure)
        {
            var field = AddField<JsonFieldProperties>(name);

            configure(new JsonFieldBuilder(field, Schema));

            return this;
        }

        public ArrayFieldBuilder AddNumber(string name, Action<NumberFieldBuilder> configure)
        {
            var field = AddField<NumberFieldProperties>(name);

            configure(new NumberFieldBuilder(field, Schema));

            return this;
        }

        public ArrayFieldBuilder AddReferences(string name, Action<ReferencesFieldBuilder> configure)
        {
            var field = AddField<ReferencesFieldProperties>(name);

            configure(new ReferencesFieldBuilder(field, Schema));

            return this;
        }

        public ArrayFieldBuilder AddString(string name, Action<StringFieldBuilder> configure)
        {
            var field = AddField<StringFieldProperties>(name);

            configure(new StringFieldBuilder(field, Schema));

            return this;
        }

        private UpsertSchemaNestedField AddField<T>(string name) where T : FieldProperties, new()
        {
            var nestedField = new UpsertSchemaNestedField
            {
                Name = name.ToCamelCase(),
                Properties = new T
                {
                    Label = name
                }
            };

            TypedField.Nested ??= Array.Empty<UpsertSchemaNestedField>();
            TypedField.Nested = TypedField.Nested.Union(new[] { nestedField }).ToArray();

            return nestedField;
        }
    }
}
