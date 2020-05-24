// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Schemas.Commands
{
    public sealed class CreateSchema : UpsertCommand
    {
        public string Name { get; set; }

        public bool IsSingleton { get; set; }

        public CreateSchema()
        {
            SchemaId = DomainId.NewGuid();
        }

        public Schema ToSchema()
        {
            return ToSchema(Name, IsSingleton);
        }
    }
}