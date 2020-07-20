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
    public class BooleanFieldBuilder : FieldBuilder
    {
        public BooleanFieldBuilder(UpsertSchemaField field, CreateSchema schema)
            : base(field, schema)
        {
        }

        public BooleanFieldBuilder AsToggle()
        {
            Properties<BooleanFieldProperties>().Editor = BooleanFieldEditor.Toggle;

            return this;
        }
    }
}
