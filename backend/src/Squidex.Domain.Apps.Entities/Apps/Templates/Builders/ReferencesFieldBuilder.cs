// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Schemas.Commands;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Apps.Templates.Builders
{
    public class ReferencesFieldBuilder : FieldBuilder
    {
        public ReferencesFieldBuilder(UpsertSchemaField field, CreateSchema schema)
            : base(field, schema)
        {
        }

        public ReferencesFieldBuilder WithSchemaId(DomainId id)
        {
            Properties<ReferencesFieldProperties>().SchemaId = id;

            return this;
        }
    }
}
