// ==========================================================================
//  RenameClient.cs
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
    public sealed class UpdateClient : AppAggregateCommand, IValidatable
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public AppClientPermission? Permission { get; set; }

        public void Validate(IList<ValidationError> errors)
        {
            if (!Id.IsSlug())
            {
                errors.Add(new ValidationError("Client id must be a valid slug.", nameof(Id)));
            }

            if (string.IsNullOrWhiteSpace(Name) && Permission == null)
            {
                errors.Add(new ValidationError("Either name or permission must be defined.", nameof(Name), nameof(Permission)));
            }

            if (Permission.HasValue && !Permission.Value.IsEnumValue())
            {
                errors.Add(new ValidationError("Permission is not valid.", nameof(Permission)));
            }
        }
    }
}
