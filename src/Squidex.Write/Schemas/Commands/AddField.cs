// ==========================================================================
//  AddField.cs
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
    public class AddField : FieldCommand, IValidatable
    {
        public string Name { get; set; }

        public FieldProperties Properties { get; set; }

        public void Validate(IList<ValidationError> errors)
        {
            if (!Name.IsPropertyName())
            {
                errors.Add(new ValidationError("DisplayName must be a valid property name", nameof(Name)));
            }

            if (Properties == null)
            {
                errors.Add(new ValidationError("Properties must be defined.", nameof(Properties)));
            }
        }
    }
}