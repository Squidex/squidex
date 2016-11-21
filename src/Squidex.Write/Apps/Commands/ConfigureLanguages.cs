// ==========================================================================
//  ConfigureLanguages.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Infrastructure;

namespace Squidex.Write.Apps.Commands
{
    public sealed class ConfigureLanguages : AppAggregateCommand, IValidatable
    {
        public List<Language> Languages { get; set; }

        public void Validate(IList<ValidationError> errors)
        {
            if (Languages == null || Languages.Count == 0)
            {
                errors.Add(new ValidationError("Languages need at least one element.", nameof(Languages)));
            }
        }
    }
}
