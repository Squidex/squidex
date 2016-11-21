// ==========================================================================
//  CreateClientKey.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Commands;

namespace Squidex.Write.Apps.Commands
{
    public sealed class CreateClientKey : AppAggregateCommand, ITimestampCommand, IValidatable
    {
        public string ClientKey { get; set; }

        public DateTime Timestamp { get; set; }

        public void Validate(IList<ValidationError> errors)
        {
            if (string.IsNullOrWhiteSpace(ClientKey))
            {
                errors.Add(new ValidationError("Client key is not assigned", nameof(ClientKey)));
            }
        }
    }
}
