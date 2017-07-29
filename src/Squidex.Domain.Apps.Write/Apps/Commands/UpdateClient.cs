// ==========================================================================
//  RenameClient.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Write.Apps.Commands
{
    public class UpdateClient : AppAggregateCommand, IValidatable
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public bool? IsReader { get; set; }

        public void Validate(IList<ValidationError> errors)
        {
            if (!Id.IsSlug())
            {
                errors.Add(new ValidationError("Client id must be a valid slug", nameof(Id)));
            }

            if (string.IsNullOrWhiteSpace(Name) && IsReader == null)
            {
                errors.Add(new ValidationError("Either name or reader state must be defined.", nameof(Name), nameof(IsReader)));
            }
        }
    }
}
