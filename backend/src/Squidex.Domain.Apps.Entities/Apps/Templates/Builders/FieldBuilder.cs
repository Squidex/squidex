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

    public abstract class FieldBuilder<T> : FieldBuilder
        where T : FieldBuilder
    {
        protected FieldBuilder(UpsertSchemaFieldBase field, CreateSchema schema)
        {
            Field = field;
            Schema = schema;
        }

        public T Label(string? label)
        {
            Field.Properties = Field.Properties with { Label = label };

            return this as T;
        }

        public T Hints(string? hints)
        {
            Field.Properties = Field.Properties with { Hints = hints };

            return this as T;
        }

        public T Localizable()
        {
            if (Field is UpsertSchemaField localizableField)
            {
                localizableField.Partitioning = Partitioning.Language.Key;
            }

            return this as T;
        }

        public T Disabled()
        {
            Field.IsDisabled = true;

            return this as T;
        }

        public T Required()
        {
            Field.Properties = Field.Properties with { IsRequired = true };

            return this as T;
        }

        protected void Properties<TProperties>(Func<TProperties, TProperties> updater) where TProperties : FieldProperties
        {
            Field.Properties = updater((T)Field.Properties);
        }

        public T ShowInList()
        {
            Schema.FieldsInLists ??= new FieldNames();
            Schema.FieldsInLists.Add(Field.Name);

            return this as T;
        }

        public T ShowInReferences()
        {
            Schema.FieldsInReferences ??= new FieldNames();
            Schema.FieldsInReferences.Add(Field.Name);

            return this as T;
        }
    }
}
