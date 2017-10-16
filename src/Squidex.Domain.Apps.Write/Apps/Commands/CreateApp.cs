// ==========================================================================
//  CreateApp.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Commands;

namespace Squidex.Domain.Apps.Write.Apps.Commands
{
    public sealed class CreateApp : SquidexCommand, IValidatable, IAggregateCommand
    {
        public string Name { get; set; }

        public Guid AppId { get; set; }

        Guid IAggregateCommand.AggregateId
        {
            get { return AppId; }
        }

        public CreateApp()
        {
            AppId = Guid.NewGuid();
        }

        public void Validate(IList<ValidationError> errors)
        {
            if (!Name.IsSlug())
            {
                errors.Add(new ValidationError("Name must be a valid slug.", nameof(Name)));
            }
        }
    }
}