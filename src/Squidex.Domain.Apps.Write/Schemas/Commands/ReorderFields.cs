// ==========================================================================
//  ReorderFields.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Write.Schemas.Commands
{
    public sealed class ReorderFields : SchemaAggregateCommand, IValidatable
    {
        public List<long> FieldIds { get; set; }

        public void Validate(IList<ValidationError> errors)
        {
            if (FieldIds == null)
            {
                errors.Add(new ValidationError("Field ids must be specified.", nameof(FieldIds)));
            }
        }
    }
}
