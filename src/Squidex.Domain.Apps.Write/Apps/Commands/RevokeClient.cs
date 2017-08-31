// ==========================================================================
//  RevokeClient.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Write.Apps.Commands
{
    public sealed class RevokeClient : AppAggregateCommand, IValidatable
    {
        public string Id { get; set; }

        public void Validate(IList<ValidationError> errors)
        {
            if (!Id.IsSlug())
            {
                errors.Add(new ValidationError("Client id must be a valid slug", nameof(Id)));
            }
        }
    }
}
