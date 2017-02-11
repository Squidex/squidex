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

namespace Squidex.Write.Schemas.Commands
{
    public class CreateSchema : AppCommand, IValidatable, IAggregateCommand
    {
        private SchemaProperties properties;

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

        public string Name { get; set; }

        public Guid SchemaId { get; set; }

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
                errors.Add(new ValidationError("DisplayName must be a valid slug", nameof(Name)));
            }
        }
    }
}