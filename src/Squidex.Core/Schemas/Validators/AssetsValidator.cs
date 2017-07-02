// ==========================================================================
//  AssetsValidator.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;

namespace Squidex.Core.Schemas.Validators
{
    public sealed class AssetsValidator : IValidator
    {
        private readonly bool isRequired;

        public AssetsValidator(bool isRequired)
        {
            this.isRequired = isRequired;
        }

        public async Task ValidateAsync(object value, ValidationContext context, Action<string> addError)
        {
            var assets = value as AssetsValue;

            if (assets == null || assets.AssetIds.Count == 0)
            {
                if (isRequired && !context.IsOptional)
                {
                    addError("<FIELD> is required");
                }

                return;
            }

            var invalidIds = await context.GetInvalidAssetIdsAsync(assets.AssetIds);

            foreach (var invalidId in invalidIds)
            {
                addError($"<FIELD> contains invalid asset '{invalidId}'");
            }
        }
    }
}
