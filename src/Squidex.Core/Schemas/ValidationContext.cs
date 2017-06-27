// ==========================================================================
//  ValidationContext.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Infrastructure;

namespace Squidex.Core.Schemas
{
    public sealed class ValidationContext
    {
        private readonly Func<Guid, Guid, Task<bool>> checkContent;
        private readonly Func<Guid, Task<bool>> checkAsset;

        public bool IsOptional { get; }

        public ValidationContext(
            Func<Guid, Guid, Task<bool>> checkContent,
            Func<Guid, Task<bool>> checkAsset)
            : this(checkContent, checkAsset, false)
        {
            
        }

        private ValidationContext(
            Func<Guid, Guid, Task<bool>> checkContent, 
            Func<Guid, Task<bool>> checkAsset, 
            bool isOptional)
        {
            Guard.NotNull(checkAsset, nameof(checkAsset));
            Guard.NotNull(checkContent, nameof(checkAsset));

            this.checkContent = checkContent;
            this.checkAsset = checkAsset;

            IsOptional = isOptional;
        }

        public ValidationContext Optional(bool isOptional)
        {
            return isOptional == IsOptional ? this : new ValidationContext(checkContent, checkAsset, isOptional);
        }

        public async Task<bool> IsValidContentIdAsync(Guid schemaId, Guid contentId)
        {
            return contentId != Guid.Empty && schemaId != Guid.Empty && await checkContent(schemaId, contentId);
        }

        public async Task<bool> IsValidAssetIdAsync(Guid assetId)
        {
            return assetId != Guid.Empty && await checkAsset(assetId);
        }
    }
}
