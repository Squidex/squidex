// ==========================================================================
//  AssetsValidator.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Linq;
using System.Threading.Tasks;

namespace Squidex.Core.Schemas.Validators
{
    public sealed class AssetsValidator : IValidator
    {
        private readonly IAssetTester assetTester;
        private readonly bool isRequired;

        public AssetsValidator(IAssetTester assetTester, bool isRequired)
        {
            this.assetTester = assetTester;
            this.isRequired = isRequired;
        }

        public async Task ValidateAsync(object value, bool isOptional, Action<string> addError)
        {
            var assets = value as AssetsValue;

            if (assets == null || assets.AssetIds.Count == 0)
            {
                if (isRequired && !isOptional)
                {
                    addError("<FIELD> is required");
                }

                return;
            }

            var assetTasks = assets.AssetIds.Select(CheckAsset).ToArray();

            await Task.WhenAll(assetTasks);

            foreach (var notFoundId in assetTasks.Where(x => !x.Result.IsFound).Select(x => x.Result.AssetId))
            {
                addError($"<FIELD> contains invalid asset '{notFoundId}'");
            }
        }

        private async Task<(Guid AssetId, bool IsFound)> CheckAsset(Guid id)
        {
            var isFound = await assetTester.IsValidAsync(id);

            return (id, isFound);
        }
    }
}
