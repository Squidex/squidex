// ==========================================================================
//  DeletePattern.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================
using System.Collections.Generic;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Write.Apps.Commands
{
    public sealed class DeletePattern : AppAggregateCommand, IValidatable
    {
        public string Name { get; set; }

        public void Validate(IList<ValidationError> errors)
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                errors.Add(new ValidationError("Name is not defined", nameof(Name)));
            }
        }
    }
}
