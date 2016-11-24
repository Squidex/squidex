// ==========================================================================
//  AttachClient.cs
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
    public sealed class AttachClient : AppAggregateCommand, ITimestampCommand, IValidatable
    {
        public string ClientName { get; set; }

        public DateTime Timestamp { get; set; }

        public void Validate(IList<ValidationError> errors)
        {
            if (!ClientName.IsSlug())
            {
                errors.Add(new ValidationError("Name must be a valid slug", nameof(ClientName)));
            }
        }
    }
}
