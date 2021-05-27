// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Schemas.Commands;

namespace Squidex.Domain.Apps.Entities.Apps.Templates.Builders
{
    public class JsonFieldBuilder : FieldBuilder<JsonFieldBuilder>
    {
        public JsonFieldBuilder(UpsertSchemaFieldBase field, CreateSchema schema)
            : base(field, schema)
        {
        }
    }
}
