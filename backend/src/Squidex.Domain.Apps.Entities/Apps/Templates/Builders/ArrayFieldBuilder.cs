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
    public sealed class ArrayFieldBuilder : FieldBuilder<ArrayFieldBuilder, ArrayFieldProperties>
    {
        private UpsertSchemaField TypedField
        {
            get => (UpsertSchemaField)Field;
        }

        public ArrayFieldBuilder(UpsertSchemaField field, ArrayFieldProperties properties, CreateSchema schema)
            : base(field, properties, schema)
        {
        }

        public ArrayFieldBuilder AddAssets(string name, Action<AssetFieldBuilder> configure)
        {
            var (field, properties) = AddField<AssetsFieldProperties>(name);

            configure(new AssetFieldBuilder(field, properties, Schema));

            return this;
        }

        public ArrayFieldBuilder AddBoolean(string name, Action<BooleanFieldBuilder> configure)
        {
            var (field, properties) = AddField<BooleanFieldProperties>(name);

            configure(new BooleanFieldBuilder(field, properties, Schema));

            return this;
        }

        public ArrayFieldBuilder AddDateTime(string name, Action<DateTimeFieldBuilder> configure)
        {
            var (field, properties) = AddField<DateTimeFieldProperties>(name);

            configure(new DateTimeFieldBuilder(field, properties, Schema));

            return this;
        }

        public ArrayFieldBuilder AddComponent(string name, Action<ComponentFieldBuilder> configure)
        {
            var (field, properties) = AddField<ComponentFieldProperties>(name);

            configure(new ComponentFieldBuilder(field, properties, Schema));

            return this;
        }

        public ArrayFieldBuilder AddComponents(string name, Action<ComponentsFieldBuilder> configure)
        {
            var (field, properties) = AddField<ComponentsFieldProperties>(name);

            configure(new ComponentsFieldBuilder(field, properties, Schema));

            return this;
        }

        public ArrayFieldBuilder AddJson(string name, Action<JsonFieldBuilder> configure)
        {
            var (field, properties) = AddField<JsonFieldProperties>(name);

            configure(new JsonFieldBuilder(field, properties, Schema));

            return this;
        }

        public ArrayFieldBuilder AddNumber(string name, Action<NumberFieldBuilder> configure)
        {
            var (field, properties) = AddField<NumberFieldProperties>(name);

            configure(new NumberFieldBuilder(field, properties, Schema));

            return this;
        }

        public ArrayFieldBuilder AddReferences(string name, Action<ReferencesFieldBuilder> configure)
        {
            var (field, properties) = AddField<ReferencesFieldProperties>(name);

            configure(new ReferencesFieldBuilder(field, properties, Schema));

            return this;
        }

        public ArrayFieldBuilder AddString(string name, Action<StringFieldBuilder> configure)
        {
            var (field, properties) = AddField<StringFieldProperties>(name);

            configure(new StringFieldBuilder(field, properties, Schema));

            return this;
        }

        public ArrayFieldBuilder AddTags(string name, Action<TagsFieldBuilder> configure)
        {
            var (field, properties) = AddField<TagsFieldProperties>(name);

            configure(new TagsFieldBuilder(field, properties, Schema));

            return this;
        }

        public ArrayFieldBuilder AddUI(string name, Action<UIFieldBuilder> configure)
        {
            var (field, properties) = AddField<UIFieldProperties>(name);

            configure(new UIFieldBuilder(field, properties, Schema));

            return this;
        }

        private (UpsertSchemaNestedField, T) AddField<T>(string name) where T : FieldProperties, new()
        {
            var properties = new T
            {
                Label = name
            };

            var nestedField = new UpsertSchemaNestedField
            {
                Name = name.ToCamelCase(),
                Properties = properties
            };

            TypedField.Nested ??= Array.Empty<UpsertSchemaNestedField>();
            TypedField.Nested = TypedField.Nested.Union(new[] { nestedField }).ToArray();

            return (nestedField, properties);
        }
    }
}
