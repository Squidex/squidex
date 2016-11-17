// ==========================================================================
//  CreateSchema.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Core.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Write.Schemas.Commands
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
                errors.Add(new ValidationError("DisplayName must be a valid slug", nameof(Name)));
            }
        }
    }
}