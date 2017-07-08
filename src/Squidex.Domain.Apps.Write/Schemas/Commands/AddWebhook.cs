// ==========================================================================
//  AddWebhook.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using Squidex.Infrastructure;

namespace Squidex.Write.Schemas.Commands
{
    public sealed class AddWebhook : SchemaAggregateCommand, IValidatable
    {
        public Guid Id { get; } = Guid.NewGuid();

        public Uri Url { get; set; }

        public string SharedSecret { get; } = RandomHash.New();

        public void Validate(IList<ValidationError> errors)
        {
            if (Url == null || !Url.IsAbsoluteUri)
            {
                errors.Add(new ValidationError("Url must be specified and absolute", nameof(Url)));
            }
        }
    }
}
