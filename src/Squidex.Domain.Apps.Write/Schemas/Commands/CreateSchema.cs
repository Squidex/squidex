// ==========================================================================
//  CreateSchema.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Commands;
using SchemaFields = System.Collections.Generic.List<Squidex.Domain.Apps.Write.Schemas.Commands.CreateSchemaField>;

namespace Squidex.Domain.Apps.Write.Schemas.Commands
{
    public sealed class CreateSchema : AppCommand, IValidatable, IAggregateCommand
    {
        private SchemaProperties properties;
        private SchemaFields fields;

        public Guid SchemaId { get; set; }

        public SchemaProperties Properties
        {
            get
            {
                return properties ?? (properties = new SchemaProperties());
            }
            set
            {
                properties = value;
            }
        }

        public SchemaFields Fields
        {
            get
            {
                return fields ?? (fields = new SchemaFields());
            }
            set
            {
                fields = value;
            }
        }

        public string Name { get; set; }

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

            var index = 0;

            foreach (var field in Fields)
            {
                field.Validate(index++, errors);
            }
        }
    }
}