// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Schemas.Commands;

namespace Squidex.Domain.Apps.Entities.Apps.Templates.Builders
{
    public class ReferencesFieldBuilder : FieldBuilder
    {
        public ReferencesFieldBuilder(UpsertSchemaField field, UpsertCommand schema)
            : base(field, schema)
        {
        }

        public ReferencesFieldBuilder WithSchemaId(Guid id)
        {
            Properties<ReferencesFieldProperties>().SchemaId = id;

            return this;
        }
    }
}
