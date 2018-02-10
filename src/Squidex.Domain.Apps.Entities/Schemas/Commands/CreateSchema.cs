// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using SchemaFields = System.Collections.Generic.List<Squidex.Domain.Apps.Entities.Schemas.Commands.CreateSchemaField>;

namespace Squidex.Domain.Apps.Entities.Schemas.Commands
{
    public sealed class CreateSchema : SchemaCommand, IAppCommand
    {
        public NamedId<Guid> AppId { get; set; }

        public string Name { get; set; }

        public SchemaFields Fields { get; set; }

        public SchemaProperties Properties { get; set; }

        public bool Publish { get; set; }

        public CreateSchema()
        {
            SchemaId = Guid.NewGuid();
        }
    }
}