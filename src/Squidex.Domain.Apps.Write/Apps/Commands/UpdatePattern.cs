// ==========================================================================
//  UpdatePattern.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================
using System;
using System.Collections.Generic;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Write.Apps.Commands
{
    public sealed class UpdatePattern : AppAggregateCommand, IValidatable
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Pattern { get; set; }

        public string DefaultMessage { get; set; }

        public void Validate(IList<ValidationError> errors)
        {
            if (Id == Guid.Empty)
            {
                errors.Add(new ValidationError("Id is empty", nameof(Id)));
            }

            if (string.IsNullOrWhiteSpace(Name))
            {
                errors.Add(new ValidationError("Name is not defined", nameof(Name)));
            }

            if (string.IsNullOrWhiteSpace(Pattern))
            {
                errors.Add(new ValidationError("Pattern is not defined", nameof(Pattern)));
            }
        }
    }
}
