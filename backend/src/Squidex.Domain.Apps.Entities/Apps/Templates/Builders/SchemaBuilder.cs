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
    public sealed class SchemaBuilder
    {
        private readonly CreateSchema command;

        public SchemaBuilder(CreateSchema command)
        {
            this.command = command;
        }

        public static SchemaBuilder Create(string name)
        {
            var schemaName = name.ToKebabCase();

            return new SchemaBuilder(new CreateSchema
            {
                Name = schemaName
            }).Published().WithLabel(name);
        }

        public SchemaBuilder WithLabel(string? label)
        {
            if (command.Properties == null)
            {
                command.Properties = new SchemaProperties { Label = label };
            }
            else
            {
                command.Properties = command.Properties with { Label = label };
            }

            return this;
        }

        public SchemaBuilder WithScripts(SchemaScripts scripts)
        {
            command.Scripts = scripts;

            return this;
        }

        public SchemaBuilder Published()
        {
            command.IsPublished = true;

            return this;
        }

        public SchemaBuilder Singleton()
        {
            command.Type = SchemaType.Singleton;

            return this;
        }

        public SchemaBuilder AddArray(string name, Action<ArrayFieldBuilder> configure)
        {
            var (field, properties) = AddField<ArrayFieldProperties>(name);

            configure(new ArrayFieldBuilder(field, properties, command));

            return this;
        }

        public SchemaBuilder AddAssets(string name, Action<AssetFieldBuilder> configure)
        {
            var (field, properties) = AddField<AssetsFieldProperties>(name);

            configure(new AssetFieldBuilder(field, properties, command));

            return this;
        }

        public SchemaBuilder AddBoolean(string name, Action<BooleanFieldBuilder> configure)
        {
            var (field, properties) = AddField<BooleanFieldProperties>(name);

            configure(new BooleanFieldBuilder(field, properties, command));

            return this;
        }

        public SchemaBuilder AddComponent(string name, Action<ComponentFieldBuilder> configure)
        {
            var (field, properties) = AddField<ComponentFieldProperties>(name);

            configure(new ComponentFieldBuilder(field, properties, command));

            return this;
        }

        public SchemaBuilder AddComponents(string name, Action<ComponentsFieldBuilder> configure)
        {
            var (field, properties) = AddField<ComponentsFieldProperties>(name);

            configure(new ComponentsFieldBuilder(field, properties, command));

            return this;
        }

        public SchemaBuilder AddDateTime(string name, Action<DateTimeFieldBuilder> configure)
        {
            var (field, properties) = AddField<DateTimeFieldProperties>(name);

            configure(new DateTimeFieldBuilder(field, properties, command));

            return this;
        }

        public SchemaBuilder AddJson(string name, Action<JsonFieldBuilder> configure)
        {
            var (field, properties) = AddField<JsonFieldProperties>(name);

            configure(new JsonFieldBuilder(field, properties, command));

            return this;
        }

        public SchemaBuilder AddNumber(string name, Action<NumberFieldBuilder> configure)
        {
            var (field, properties) = AddField<NumberFieldProperties>(name);

            configure(new NumberFieldBuilder(field, properties, command));

            return this;
        }

        public SchemaBuilder AddReferences(string name, Action<ReferencesFieldBuilder> configure)
        {
            var (field, properties) = AddField<ReferencesFieldProperties>(name);

            configure(new ReferencesFieldBuilder(field, properties, command));

            return this;
        }

        public SchemaBuilder AddString(string name, Action<StringFieldBuilder> configure)
        {
            var (field, properties) = AddField<StringFieldProperties>(name);

            configure(new StringFieldBuilder(field, properties, command));

            return this;
        }

        public SchemaBuilder AddTags(string name, Action<TagsFieldBuilder> configure)
        {
            var (field, properties) = AddField<TagsFieldProperties>(name);

            configure(new TagsFieldBuilder(field, properties, command));

            return this;
        }

        public SchemaBuilder AddUI(string name, Action<UIFieldBuilder> configure)
        {
            var (field, properties) = AddField<UIFieldProperties>(name);

            configure(new UIFieldBuilder(field, properties, command));

            return this;
        }

        private (UpsertSchemaField, T) AddField<T>(string name) where T : FieldProperties, new()
        {
            var properties = new T { Label = name };

            var field = new UpsertSchemaField
            {
                Name = name.ToCamelCase(),
                Properties = properties,
            };

            if (command.Fields == null)
            {
                command.Fields = new[] { field };
            }
            else
            {
                command.Fields = command.Fields.Union(Enumerable.Repeat(field, 1)).ToArray();
            }

            return (field, properties);
        }

        public CreateSchema Build()
        {
            return command;
        }
    }
}
