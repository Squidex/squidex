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
    public sealed class ComponentFieldBuilder : FieldBuilder<ComponentFieldBuilder, ComponentFieldProperties>
    {
        public ComponentFieldBuilder(UpsertSchemaFieldBase field, ComponentFieldProperties properties, CreateSchema schema)
            : base(field, properties, schema)
        {
        }
    }
}
