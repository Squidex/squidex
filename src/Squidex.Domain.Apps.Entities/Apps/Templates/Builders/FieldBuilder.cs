// ==========================================================================
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
        private readonly CreateSchemaField field;

        protected T Properties<T>() where T : FieldProperties
        {
            return field.Properties as T;
        }

        protected FieldBuilder(CreateSchemaField field)
        {
            this.field = field;
        }

        public FieldBuilder Label(string label)
        {
            field.Properties.Label = label;

            return this;
        }

        public FieldBuilder Hints(string hints)
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
            field.Properties.IsListField = true;

            return this;
        }
    }
}
