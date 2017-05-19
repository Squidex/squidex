// ==========================================================================
//  CreateSchema.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using Squidex.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Commands;

using SchemaFields = System.Collections.Generic.List<Squidex.Write.Schemas.Commands.CreateSchemaField>;

namespace Squidex.Write.Schemas.Commands
{
    public class CreateSchema : AppCommand, IValidatable, IAggregateCommand
    {
        public Guid SchemaId { get; set; }

        public string Name { get; set; }

        public SchemaFields Fields { get; set; } = new SchemaFields();

        public SchemaProperties Properties { get; set; }

        Guid IAggregateCommand.AggregateId
        {
            get { return SchemaId; }
        }

        public CreateSchema()
        {
            SchemaId = Guid.NewGuid();
        }

        public void Validate(IList<ValidationError> errors)
        {
            if (!Name.IsSlug())
            {
                errors.Add(new ValidationError("Name must be a valid slug", nameof(Name)));
            }

            if (Properties == null)
            {
                errors.Add(new ValidationError("Properties must be specified", nameof(Properties)));
            }
        }
    }
}