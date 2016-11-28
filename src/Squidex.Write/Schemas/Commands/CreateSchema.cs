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

namespace Squidex.Write.Schemas.Commands
{
    public class CreateSchema : AppCommand, IValidatable
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

        public CreateSchema()
        {
            AggregateId = Guid.NewGuid();
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