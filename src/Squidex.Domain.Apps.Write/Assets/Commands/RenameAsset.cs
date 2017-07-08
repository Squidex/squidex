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
        public string FileName { get; set; }

        public void Validate(IList<ValidationError> errors)
        {
            if (string.IsNullOrWhiteSpace(FileName))
            {
                errors.Add(new ValidationError("File name must not be null or empty.", nameof(FileName)));
            }
        }
    }
}
