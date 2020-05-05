// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Infrastructure;

namespace Squidex.Web.Pipeline
{
    public sealed class SchemaFeature : ISchemaFeature
    {
        public NamedId<Guid> SchemaId { get; }

        public SchemaFeature(NamedId<Guid> schemaId)
        {
            SchemaId = schemaId;
        }
    }
}
