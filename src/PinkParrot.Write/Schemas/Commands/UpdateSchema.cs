// ==========================================================================
//  UpdateSchema.cs
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
    public class UpdateSchema : AppCommand, IValidatable
    {
        public SchemaProperties Properties { get; set; }

        public void Validate(IList<ValidationError> errors)
        {
            if (Properties == null)
            {
                errors.Add(new ValidationError("Properties must be specified", nameof(Properties)));
            }
        }
    }
}