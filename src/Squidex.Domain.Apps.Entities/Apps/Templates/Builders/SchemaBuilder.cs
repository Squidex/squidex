﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Schemas.Commands;
using Squidex.Infrastructure;

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
            command.Properties ??= new SchemaProperties();
            command.Properties.Label = label;

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
            command.IsSingleton = true;

            return this;
        }

        public SchemaBuilder AddAssets(string name, Action<AssetFieldBuilder> configure)
        {
            var field = AddField<AssetsFieldProperties>(name);

            configure(new AssetFieldBuilder(field));

            return this;
        }

        public SchemaBuilder AddBoolean(string name, Action<BooleanFieldBuilder> configure)
        {
            var field = AddField<BooleanFieldProperties>(name);

            configure(new BooleanFieldBuilder(field));

            return this;
        }

        public SchemaBuilder AddDateTime(string name, Action<DateTimeFieldBuilder> configure)
        {
            var field = AddField<DateTimeFieldProperties>(name);

            configure(new DateTimeFieldBuilder(field));

            return this;
        }

        public SchemaBuilder AddJson(string name, Action<JsonFieldBuilder> configure)
        {
            var field = AddField<JsonFieldProperties>(name);

            configure(new JsonFieldBuilder(field));

            return this;
        }

        public SchemaBuilder AddNumber(string name, Action<NumberFieldBuilder> configure)
        {
            var field = AddField<NumberFieldProperties>(name);

            configure(new NumberFieldBuilder(field));

            return this;
        }

        public SchemaBuilder AddString(string name, Action<StringFieldBuilder> configure)
        {
            var field = AddField<StringFieldProperties>(name);

            configure(new StringFieldBuilder(field));

            return this;
        }

        public SchemaBuilder AddTags(string name, Action<TagsFieldBuilder> configure)
        {
            var field = AddField<TagsFieldProperties>(name);

            configure(new TagsFieldBuilder(field));

            return this;
        }

        private UpsertSchemaField AddField<T>(string name) where T : FieldProperties, new()
        {
            var field = new UpsertSchemaField
            {
                Name = name.ToCamelCase(),
                Properties = new T
                {
                    Label = name
                }
            };

            command.Fields ??= new List<UpsertSchemaField>();
            command.Fields.Add(field);

            return field;
        }

        public CreateSchema Build()
        {
            return command;
        }
    }
}
