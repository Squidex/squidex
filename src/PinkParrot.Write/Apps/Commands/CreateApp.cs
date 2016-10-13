// ==========================================================================
//  CreateApp.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using PinkParrot.Infrastructure;
using PinkParrot.Infrastructure.CQRS.Commands;

namespace PinkParrot.Write.Apps.Commands
{
    public sealed class CreateApp : AggregateCommand, IValidatable
    {
        public string Name { get; set; }

        public void Validate(IList<ValidationError> errors)
        {
            if (!Name.IsSlug())
            {
                errors.Add(new ValidationError("Name must be a valid slug", nameof(Name)));
            }
        }
    }
}