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
        protected readonly CreateSchema schema;

        protected FieldBuilder(UpsertSchemaFieldBase field, CreateSchema schema)
        {
            this.field = field;
            this.schema = schema;
        }

        public FieldBuilder Label(string? label)
        {
            field.Properties = field.Properties with { Label = label };

            return this;
        }

        public FieldBuilder Hints(string? hints)
        {
            field.Properties = field.Properties with { Hints = hints };

            return this;
        }

        public FieldBuilder Localizable()
        {
            if (field is UpsertSchemaField localizableField)
            {
                localizableField.Partitioning = Partitioning.Language.Key;
            }

            return this;
        }

        public FieldBuilder Disabled()
        {
            field.IsDisabled = true;

            return this;
        }

        public FieldBuilder Required()
        {
            field.Properties = field.Properties with { IsRequired = true };

            return this;
        }

        protected void Properties<T>(Func<T, T> updater) where T : FieldProperties
        {
            field.Properties = updater((T)field.Properties);
        }

        public FieldBuilder ShowInList()
        {
            schema.FieldsInLists ??= new FieldNames();
            schema.FieldsInLists.Add(field.Name);

            return this;
        }

        public FieldBuilder ShowInReferences()
        {
            schema.FieldsInReferences ??= new FieldNames();
            schema.FieldsInReferences.Add(field.Name);

            return this;
        }
    }
}
