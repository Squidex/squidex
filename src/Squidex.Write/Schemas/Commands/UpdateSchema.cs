// ==========================================================================
//  UpdateSchema.cs
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