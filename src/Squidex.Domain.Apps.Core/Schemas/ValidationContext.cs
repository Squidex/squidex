// ==========================================================================
//  ValidationContext.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Schemas
{
    public sealed class ValidationContext
    {
        private readonly Func<IEnumerable<Guid>, Guid, Task<IReadOnlyList<Guid>>> checkContent;
        private readonly Func<IEnumerable<Guid>, Task<IReadOnlyList<Guid>>> checkAsset;

        public bool IsOptional { get; }

        public ValidationContext(
            Func<IEnumerable<Guid>, Guid, Task<IReadOnlyList<Guid>>> checkContent,
            Func<IEnumerable<Guid>, Task<IReadOnlyList<Guid>>> checkAsset)
            : this(checkContent, checkAsset, false)
        {

        }

        private ValidationContext(
            Func<IEnumerable<Guid>, Guid, Task<IReadOnlyList<Guid>>> checkContent,
            Func<IEnumerable<Guid>, Task<IReadOnlyList<Guid>>> checkAsset,
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

        public Task<IReadOnlyList<Guid>> GetInvalidContentIdsAsync(IEnumerable<Guid> contentIds, Guid schemaId)
        {
            return checkContent(contentIds, schemaId);
        }

        public Task<IReadOnlyList<Guid>> GetInvalidAssetIdsAsync(IEnumerable<Guid> assetId)
        {
            return checkAsset(assetId);
        }
    }
}
