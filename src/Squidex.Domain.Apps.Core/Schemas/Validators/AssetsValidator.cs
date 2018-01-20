// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Squidex.Domain.Apps.Core.Schemas.Validators
{
    public sealed class AssetsValidator : IValidator
    {
        public async Task ValidateAsync(object value, ValidationContext context, Action<string> addError)
        {
            if (value is ICollection<Guid> assetIds)
            {
                var invalidIds = await context.GetInvalidAssetIdsAsync(assetIds);

                foreach (var invalidId in invalidIds)
                {
                    addError($"<FIELD> contains invalid asset '{invalidId}'.");
                }
            }
        }
    }
}
