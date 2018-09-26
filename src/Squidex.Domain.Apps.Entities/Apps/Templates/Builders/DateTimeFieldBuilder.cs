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
    public class DateTimeFieldBuilder : FieldBuilder
    {
        public DateTimeFieldBuilder(CreateSchemaField field)
            : base(field)
        {
        }

        public DateTimeFieldBuilder AsDateTime()
        {
            Properties<DateTimeFieldProperties>().Editor = DateTimeFieldEditor.DateTime;

            return this;
        }
    }
}
