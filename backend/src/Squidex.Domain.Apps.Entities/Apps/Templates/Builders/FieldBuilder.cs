﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Schemas.Commands;

namespace Squidex.Domain.Apps.Entities.Apps.Templates.Builders
{
    public abstract class FieldBuilder
    {
        private readonly UpsertSchemaField field;
        private readonly UpsertCommand schema;

        protected T Properties<T>() where T : FieldProperties
        {
            return (T)field.Properties;
        }

        protected FieldBuilder(UpsertSchemaField field, UpsertCommand schema)
        {
            this.field = field;
            this.schema = schema;
        }

        public FieldBuilder Label(string? label)
        {
            field.Properties.Label = label;

            return this;
        }

        public FieldBuilder Hints(string? hints)
        {
            field.Properties.Hints = hints;

            return this;
        }

        public FieldBuilder Localizable()
        {
            field.Partitioning = Partitioning.Language.Key;

            return this;
        }

        public FieldBuilder Disabled()
        {
            field.IsDisabled = true;

            return this;
        }

        public FieldBuilder Required()
        {
            field.Properties.IsRequired = true;

            return this;
        }

        public FieldBuilder ShowInList()
        {
            if (schema.FieldsInReferences == null)
            {
                schema.FieldsInReferences = new FieldNames();
            }

            schema.FieldsInReferences.Add(field.Name);

            return this;
        }

        public FieldBuilder ShowInReferences()
        {
            if (schema.FieldsInLists == null)
            {
                schema.FieldsInLists = new FieldNames();
            }

            schema.FieldsInLists.Add(field.Name);

            return this;
        }
    }
}
