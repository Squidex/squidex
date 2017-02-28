// ==========================================================================
//  CreateAsset.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Infrastructure;

namespace Squidex.Write.Assets.Commands
{
    public sealed class CreateAsset : AssetAggregateCommand, IValidatable
    {
        public string Name { get; set; }

        public void Validate(IList<ValidationError> errors)
        {
            if (!Name.IsSlug())
            {
                errors.Add(new ValidationError("Name must be a valid slug", nameof(Name)));
            }
        }
    }
}
