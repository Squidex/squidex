// ==========================================================================
//  ChangePlan.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Write.Apps.Commands
{
    public sealed class ChangePlan : AppAggregateCommand, IValidatable
    {
        public string PlanId { get; set; }

        public void Validate(IList<ValidationError> errors)
        {
            if (string.IsNullOrWhiteSpace(PlanId))
            {
                errors.Add(new ValidationError("PlanId is not defined", nameof(PlanId)));
            }
        }
    }
}
