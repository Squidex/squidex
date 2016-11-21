// ==========================================================================
//  RevokeClientKey.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Infrastructure;

namespace Squidex.Write.Apps.Commands
{
    public class RevokeClientKey : AppAggregateCommand, IValidatable
    {
        public string ClientKey { get; set; }

        public void Validate(IList<ValidationError> errors)
        {
            if (string.IsNullOrWhiteSpace(ClientKey))
            {
                errors.Add(new ValidationError("Client key is not assigned", nameof(ClientKey)));
            }
        }
    }
}
