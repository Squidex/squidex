// ==========================================================================
//  CreateSchema.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using PinkParrot.Core.Schemas;
using PinkParrot.Infrastructure;

namespace PinkParrot.Write.Schemas.Commands
{
    public class CreateSchema : AppCommand, IValidatable
    {
        private SchemaProperties properties;

        public string Name { get; set; }

        public SchemaProperties Properties
        {
            get
            {
                return properties ?? (properties = new SchemaProperties(null, null)); 
            }
            set { properties = value; }
        }

        public void Validate(IList<ValidationError> errors)
        {
            if (!Name.IsSlug())
            {
                errors.Add(new ValidationError("Name must be a valid slug", nameof(Name)));
            }
        }
    }
}