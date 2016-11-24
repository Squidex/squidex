// ==========================================================================
//  RevokeClient.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Infrastructure;

namespace Squidex.Write.Apps.Commands
{
    public class RevokeClient : AppAggregateCommand, IValidatable
    {
        public string ClientName { get; set; }

        public void Validate(IList<ValidationError> errors)
        {
            if (!ClientName.IsSlug())
            {
                errors.Add(new ValidationError("Name must be a valid slug", nameof(ClientName)));
            }
        }
    }
}
