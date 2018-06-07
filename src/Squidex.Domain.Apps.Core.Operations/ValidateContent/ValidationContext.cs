// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.ValidateContent
{
    public sealed class ValidationContext
    {
        private readonly Func<IEnumerable<Guid>, Guid, Task<IReadOnlyList<Guid>>> checkContent;
        private readonly Func<IEnumerable<Guid>, Task<IReadOnlyList<IAssetInfo>>> checkAsset;
        private readonly ImmutableQueue<string> propertyPath;

        public ImmutableQueue<string> Path
        {
            get { return propertyPath; }
        }

        public bool IsOptional { get; }

        public ValidationContext(
            Func<IEnumerable<Guid>, Guid, Task<IReadOnlyList<Guid>>> checkContent,
            Func<IEnumerable<Guid>, Task<IReadOnlyList<IAssetInfo>>> checkAsset)
            : this(checkContent, checkAsset, ImmutableQueue<string>.Empty, false)
        {
        }

        private ValidationContext(
            Func<IEnumerable<Guid>, Guid, Task<IReadOnlyList<Guid>>> checkContent,
            Func<IEnumerable<Guid>, Task<IReadOnlyList<IAssetInfo>>> checkAsset,
            ImmutableQueue<string> propertyPath,
            bool isOptional)
        {
            Guard.NotNull(checkAsset, nameof(checkAsset));
            Guard.NotNull(checkContent, nameof(checkAsset));

            this.propertyPath = propertyPath;

            this.checkContent = checkContent;
            this.checkAsset = checkAsset;

            IsOptional = isOptional;
        }

        public ValidationContext Optional(bool isOptional)
        {
            return isOptional == IsOptional ? this : new ValidationContext(checkContent, checkAsset, propertyPath, isOptional);
        }

        public ValidationContext Nested(string property)
        {
            return new ValidationContext(checkContent, checkAsset, propertyPath.Enqueue(property), IsOptional);
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
