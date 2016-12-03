// ==========================================================================
//  RenameClient.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Infrastructure;

namespace Squidex.Write.Apps.Commands
{
    public class RenameClient : AppAggregateCommand, IValidatable
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public void Validate(IList<ValidationError> errors)
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                errors.Add(new ValidationError("Name cannot be null or empty", nameof(Name)));
            }

            if (!Id.IsSlug())
            {
                errors.Add(new ValidationError("Client id must be a valid slug", nameof(Id)));
            }
        }
    }
}
