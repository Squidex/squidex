// ==========================================================================
//  CreateSchema.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure.Commands;
using SchemaFields = System.Collections.Generic.List<Squidex.Domain.Apps.Entities.Schemas.Commands.CreateSchemaField>;

namespace Squidex.Domain.Apps.Entities.Schemas.Commands
{
    public sealed class CreateSchema : AppCommand, IAggregateCommand
    {
        public Guid SchemaId { get; set; }

        public SchemaFields Fields { get; set; }

        public SchemaProperties Properties { get; set; }

        public string Name { get; set; }

        Guid IAggregateCommand.AggregateId
        {
            get { return SchemaId; }
        }

        public CreateSchema()
        {
            SchemaId = Guid.NewGuid();
        }
    }
}