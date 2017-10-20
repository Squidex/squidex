// ==========================================================================
//  AttachClient.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Write.Apps.Commands
{
    public sealed class AttachClient : AppAggregateCommand, IValidatable
    {
        public string Id { get; set; }

        public string Secret { get; } = RandomHash.New();

        public void Validate(IList<ValidationError> errors)
        {
            if (!Id.IsSlug())
            {
                errors.Add(new ValidationError("Client id must be a valid slug.", nameof(Id)));
            }
        }
    }
}
