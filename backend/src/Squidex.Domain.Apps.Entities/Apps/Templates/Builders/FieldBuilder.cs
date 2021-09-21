// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Schemas.Commands;

namespace Squidex.Domain.Apps.Entities.Apps.Templates.Builders
{
    public abstract class FieldBuilder
    {
        protected UpsertSchemaFieldBase Field { get; init; }

        protected CreateSchema Schema { get; init; }
    }

    public abstract class FieldBuilder<T, TProperties> : FieldBuilder where T : FieldBuilder where TProperties : FieldProperties
    {
        private TProperties properties;

        protected FieldBuilder(UpsertSchemaFieldBase field, TProperties properties, CreateSchema schema)
        {
            this.properties = properties;

            Field = field;
            Schema = schema;
        }

        public T Localizable()
        {
            if (Field is UpsertSchemaField localizableField)
            {
                localizableField.Partitioning = Partitioning.Language.Key;
            }

            return (T)(object)this;
        }

        public T Disabled(bool isDisabled = true)
        {
            Field.IsDisabled = isDisabled;

            return (T)(object)this;
        }

        public T Hidden(bool isHidden = true)
        {
            Field.IsHidden = isHidden;

            return (T)(object)this;
        }

        public T Label(string? label)
        {
            return Properties(x => x with { Label = label });
        }

        public T Hints(string? hints)
        {
            return Properties(x => x with { Hints = hints });
        }

        public T Required(bool isRequired = true)
        {
            return Properties(x => x with { IsRequired = isRequired });
        }

        public T Properties(Func<TProperties, TProperties> updater)
        {
            properties = updater(properties);

            Field.Properties = properties;

            return (T)(object)this;
        }

        public T ShowInList()
        {
            Schema.FieldsInLists ??= new FieldNames();
            Schema.FieldsInLists.Add(Field.Name);

            return (T)(object)this;
        }

        public T ShowInReferences()
        {
            Schema.FieldsInReferences ??= new FieldNames();
            Schema.FieldsInReferences.Add(Field.Name);

            return (T)(object)this;
        }
    }
}
