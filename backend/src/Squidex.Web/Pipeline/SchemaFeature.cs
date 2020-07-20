// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Web.Pipeline
{
    public sealed class SchemaFeature : ISchemaFeature
    {
        public NamedId<DomainId> SchemaId { get; }

        public SchemaFeature(NamedId<DomainId> schemaId)
        {
            SchemaId = schemaId;
        }
    }
}
