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
    public class ArrayFieldBuilder : FieldBuilder
    {
        protected new UpsertSchemaField field
        {
            get => base.field as UpsertSchemaField;
            init => base.field = value;
        }

        public ArrayFieldBuilder(UpsertSchemaField field, CreateSchema schema)
            : base(field, schema)
        {
        }

        public new ArrayFieldBuilder Label(string? label) => base.Label(label) as ArrayFieldBuilder;
        public new ArrayFieldBuilder Hints(string? hints) => base.Hints(hints) as ArrayFieldBuilder;
        public new ArrayFieldBuilder Localizable() => base.Localizable() as ArrayFieldBuilder;
        public new ArrayFieldBuilder Disabled() => base.Disabled() as ArrayFieldBuilder;
        public new ArrayFieldBuilder Required() => base.Required() as ArrayFieldBuilder;
        public new ArrayFieldBuilder ShowInList() => base.ShowInList() as ArrayFieldBuilder;
        public new ArrayFieldBuilder ShowInReferences() => base.ShowInReferences() as ArrayFieldBuilder;

        public ArrayFieldBuilder RequireSingle()
        {
            Properties<ArrayFieldProperties>(p => p with
            {
                MaxItems = 1
            });

            return this;
        }

        public ArrayFieldBuilder AddAssets(string name, Action<AssetFieldBuilder> configure)
        {
            var field = AddField<AssetsFieldProperties>(name);
            configure(new AssetFieldBuilder(field, schema));
            return this;

        }

        public ArrayFieldBuilder AddBoolean(string name, Action<BooleanFieldBuilder> configure)
        {
            var field = AddField<BooleanFieldProperties>(name);
            configure(new BooleanFieldBuilder(field, schema));
            return this;
        }

        public ArrayFieldBuilder AddDateTime(string name, Action<DateTimeFieldBuilder> configure)
        {
            var field = AddField<DateTimeFieldProperties>(name);
            configure(new DateTimeFieldBuilder(field, schema));
            return this;
        }

        public ArrayFieldBuilder AddJson(string name, Action<JsonFieldBuilder> configure)
        {
            var field = AddField<JsonFieldProperties>(name);

            configure(new JsonFieldBuilder(field, schema));

            return this;
        }

        public ArrayFieldBuilder AddNumber(string name, Action<NumberFieldBuilder> configure)
        {
            var field = AddField<NumberFieldProperties>(name);
            configure(new NumberFieldBuilder(field, schema));
            return this;
        }

        public ArrayFieldBuilder AddReferences(string name, Action<ReferencesFieldBuilder> configure)
        {
            var field = AddField<ReferencesFieldProperties>(name);
            configure(new ReferencesFieldBuilder(field, schema));
            return this;
        }

        public ArrayFieldBuilder AddString(string name, Action<StringFieldBuilder> configure)
        {
            var field = AddField<StringFieldProperties>(name);
            configure(new StringFieldBuilder(field, schema));
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

            field.Nested ??= Array.Empty<UpsertSchemaNestedField>();
            field.Nested = field.Nested.Union(new[] { nestedField }).ToArray();

            return nestedField;
        }

    }
}
