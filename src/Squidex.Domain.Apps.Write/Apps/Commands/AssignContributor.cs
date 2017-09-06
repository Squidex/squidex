// ==========================================================================
//  AssignContributor.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Write.Apps.Commands
{
    public sealed class AssignContributor : AppAggregateCommand, IValidatable
    {
        public string ContributorId { get; set; }

        public PermissionLevel Permission { get; set; }

        public void Validate(IList<ValidationError> errors)
        {
            if (string.IsNullOrWhiteSpace(ContributorId))
            {
                errors.Add(new ValidationError("Contributor id not assigned", nameof(ContributorId)));
            }
        }
    }
}