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
        protected UpsertSchemaFieldBase field { get; init; }
        protected CreateSchema schema { get; init; }
    }

    public abstract class FieldBuilder<T> : FieldBuilder
        where T : FieldBuilder
    {
        protected FieldBuilder(UpsertSchemaFieldBase field, CreateSchema schema)
        {
            this.field = field;
            this.schema = schema;
        }

        public T Label(string? label)
        {
            field.Properties = field.Properties with { Label = label };

            return this as T;
        }

        public T Hints(string? hints)
        {
            field.Properties = field.Properties with { Hints = hints };

            return this as T;
        }

        public T Localizable()
        {
            if (field is UpsertSchemaField localizableField)
            {
                localizableField.Partitioning = Partitioning.Language.Key;
            }

            return this as T;
        }

        public T Disabled()
        {
            field.IsDisabled = true;

            return this as T;
        }

        public T Required()
        {
            field.Properties = field.Properties with { IsRequired = true };

            return this as T;
        }

        protected void Properties<T>(Func<T, T> updater) where T : FieldProperties
        {
            field.Properties = updater((T)field.Properties);
        }

        public T ShowInList()
        {
            schema.FieldsInLists ??= new FieldNames();
            schema.FieldsInLists.Add(field.Name);

            return this as T;
        }

        public T ShowInReferences()
        {
            schema.FieldsInReferences ??= new FieldNames();
            schema.FieldsInReferences.Add(field.Name);

            return this as T;
        }
    }
}
