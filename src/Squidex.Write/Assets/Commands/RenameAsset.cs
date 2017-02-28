// ==========================================================================
//  RenameAsset.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Infrastructure;

namespace Squidex.Write.Assets.Commands
{
    public sealed class RenameAsset : AssetAggregateCommand, IValidatable
    {
        public string Name { get; set; }

        public void Validate(IList<ValidationError> errors)
        {
            if (!string.IsNullOrWhiteSpace(Name))
            {
                errors.Add(new ValidationError("Name must not be null or empty.", nameof(Name)));
            }
        }
    }
}
