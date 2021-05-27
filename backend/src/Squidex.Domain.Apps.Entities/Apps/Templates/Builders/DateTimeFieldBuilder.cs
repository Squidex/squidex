// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Schemas.Commands;

namespace Squidex.Domain.Apps.Entities.Apps.Templates.Builders
{
    public class DateTimeFieldBuilder : FieldBuilder<DateTimeFieldBuilder>
    {
        public DateTimeFieldBuilder(UpsertSchemaFieldBase field, CreateSchema schema)
            : base(field, schema)
        {
        }

        public DateTimeFieldBuilder AsDateTime()
        {
            Properties<DateTimeFieldProperties>(p => p with
            {
                Editor = DateTimeFieldEditor.DateTime,
                EditorUrl = null
            });

            return this;
        }
    }
}
