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

        public string Partitioning { get; set; }

        public FieldProperties Properties { get; set; }

        public void Validate(IList<ValidationError> errors)
        {
            if (Partitioning != null && Partitioning != "language")
            {
                errors.Add(new ValidationError("Partitioning must be invariant or language.", nameof(Partitioning)));
            }

            if (!Name.IsPropertyName())
            {
                errors.Add(new ValidationError("Name must be a valid property name", nameof(Name)));
            }

            if (Properties == null)
            {
                errors.Add(new ValidationError("Properties must be defined.", nameof(Properties)));
            }
        }
    }
}