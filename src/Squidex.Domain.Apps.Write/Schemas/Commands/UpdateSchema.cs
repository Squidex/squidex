// ==========================================================================
//  UpdateSchema.cs
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
    public sealed class UpdateSchema : SchemaAggregateCommand, IValidatable
    {
        public SchemaProperties Properties { get; set; }

        public void Validate(IList<ValidationError> errors)
        {
            if (Properties == null)
            {
                errors.Add(new ValidationError("Properties must be specified.", nameof(Properties)));
            }
        }
    }
}