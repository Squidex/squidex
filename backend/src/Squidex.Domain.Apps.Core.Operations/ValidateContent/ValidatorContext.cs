// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.ValidateContent
{
    public abstract class ValidatorContext
    {
        public NamedId<DomainId> AppId { get; }

        public NamedId<DomainId> SchemaId { get; }

        public Schema Schema { get; }

        public ValidationMode Mode { get; protected set; }

        public ValidationAction Action { get; protected set; }

        protected ValidatorContext(
            NamedId<DomainId> appId,
            NamedId<DomainId> schemaId,
            Schema schema)
        {
            AppId = appId;

            Schema = schema;
            SchemaId = schemaId;
        }
    }
}
