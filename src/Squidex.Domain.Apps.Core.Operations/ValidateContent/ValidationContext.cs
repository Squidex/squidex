// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.ValidateContent
{
    public sealed class ValidationContext
    {
        private readonly Func<IEnumerable<Guid>, Guid, Task<IReadOnlyList<Guid>>> checkContent;
        private readonly Func<IEnumerable<Guid>, Task<IReadOnlyList<IAssetInfo>>> checkAsset;

        public bool IsOptional { get; }

        public ValidationContext(
            Func<IEnumerable<Guid>, Guid, Task<IReadOnlyList<Guid>>> checkContent,
            Func<IEnumerable<Guid>, Task<IReadOnlyList<IAssetInfo>>> checkAsset)
            : this(checkContent, checkAsset, false)
        {
        }

        private ValidationContext(
            Func<IEnumerable<Guid>, Guid, Task<IReadOnlyList<Guid>>> checkContent,
            Func<IEnumerable<Guid>, Task<IReadOnlyList<IAssetInfo>>> checkAsset,
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

        public Task<IReadOnlyList<IAssetInfo>> GetAssetInfosAsync(IEnumerable<Guid> assetId)
        {
            return checkAsset(assetId);
        }
    }
}
