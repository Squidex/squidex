// ==========================================================================
//  DeletePattern.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Write.Apps.Commands
{
    public sealed class DeletePattern : AppAggregateCommand, IValidatable
    {
        public Guid Id { get; set; }

        public void Validate(IList<ValidationError> errors)
        {
            if (Id == Guid.Empty)
            {
                errors.Add(new ValidationError("Id is not defined", nameof(Id)));
            }
        }
    }
}
