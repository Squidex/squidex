// ==========================================================================
//  AssetsValidator.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;

namespace Squidex.Domain.Apps.Core.Schemas.Validators
{
    public sealed class AssetsValidator : IValidator
    {
        private readonly bool isRequired;
        private readonly int? minItems;
        private readonly int? maxItems;

        public AssetsValidator(bool isRequired, int? minItems = null, int? maxItems = null)
        {
            this.isRequired = isRequired;
            this.minItems = minItems;
            this.maxItems = maxItems;
        }

        public async Task ValidateAsync(object value, ValidationContext context, Action<string> addError)
        {
            if (!(value is AssetsValue assets) || assets.AssetIds.Count == 0)
            {
                if (isRequired && !context.IsOptional)
                {
                    addError("<FIELD> is required");
                }

                return;
            }

            if (minItems.HasValue && assets.AssetIds.Count < minItems.Value)
            {
                addError($"<FIELD> must have at least {minItems} asset(s)");
            }

            if (maxItems.HasValue && assets.AssetIds.Count > maxItems.Value)
            {
                addError($"<FIELD> must have not more than {maxItems} asset(s)");
            }

            var invalidIds = await context.GetInvalidAssetIdsAsync(assets.AssetIds);

            foreach (var invalidId in invalidIds)
            {
                addError($"<FIELD> contains invalid asset '{invalidId}'");
            }
        }
    }
}
