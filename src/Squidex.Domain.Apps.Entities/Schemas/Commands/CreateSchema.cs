// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Schemas.Commands
{
    public sealed class CreateSchema : UpsertCommand, IAppCommand
    {
        public NamedId<Guid> AppId { get; set; }

        public string Name { get; set; }

        public bool IsSingleton { get; set; }

        public CreateSchema()
        {
            SchemaId = Guid.NewGuid();
        }

        public Schema ToSchema()
        {
            return ToSchema(Name, IsSingleton);
        }
    }
}