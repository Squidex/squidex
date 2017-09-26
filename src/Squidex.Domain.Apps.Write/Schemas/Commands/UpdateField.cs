// ==========================================================================
//  UpdateField.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Write.Schemas.Commands
{
    public sealed class UpdateField : FieldCommand, IValidatable
    {
        public FieldProperties Properties { get; set; }

        public void Validate(IList<ValidationError> errors)
        {
            if (Properties == null)
            {
                errors.Add(new ValidationError("Properties must be defined.", nameof(Properties)));
            }
        }
    }
}