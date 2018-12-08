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
using Squidex.Infrastructure.Queries;

namespace Squidex.Domain.Apps.Core.ValidateContent
{
    public delegate Task<IReadOnlyList<Guid>> CheckContents(Guid schemaId, FilterNode filter);

    public delegate Task<IReadOnlyList<IAssetInfo>> CheckAssets(IEnumerable<Guid> ids);

    public sealed class ValidationContext
    {
        private readonly Guid contentId;
        private readonly Guid schemaId;
        private readonly CheckContents checkContent;
        private readonly CheckAssets checkAsset;
        private readonly ImmutableQueue<string> propertyPath;

        public ImmutableQueue<string> Path
        {
            get { return propertyPath; }
        }

        public Guid ContentId
        {
            get { return contentId; }
        }

        public Guid SchemaId
        {
            get { return schemaId; }
        }

        public bool IsOptional { get; }

        public ValidationContext(
            Guid contentId,
            Guid schemaId,
            CheckContents checkContent,
            CheckAssets checkAsset)
            : this(contentId, schemaId, checkContent, checkAsset, ImmutableQueue<string>.Empty, false)
        {
        }

        private ValidationContext(
            Guid contentId,
            Guid schemaId,
            CheckContents checkContent,
            CheckAssets checkAsset,
            ImmutableQueue<string> propertyPath,
            bool isOptional)
        {
            Guard.NotNull(checkAsset, nameof(checkAsset));
            Guard.NotNull(checkContent, nameof(checkAsset));

            this.propertyPath = propertyPath;

            this.checkContent = checkContent;
            this.checkAsset = checkAsset;
            this.contentId = contentId;

            this.schemaId = schemaId;

            IsOptional = isOptional;
        }

        public ValidationContext Optional(bool isOptional)
        {
            return isOptional == IsOptional ? this : new ValidationContext(contentId, schemaId, checkContent, checkAsset, propertyPath, isOptional);
        }

        public ValidationContext Nested(string property)
        {
            return new ValidationContext(contentId, schemaId, checkContent, checkAsset, propertyPath.Enqueue(property), IsOptional);
        }

        public Task<IReadOnlyList<Guid>> GetContentIdsAsync(Guid schemaId, FilterNode filter)
        {
            return checkContent(schemaId, filter);
        }

        public Task<IReadOnlyList<IAssetInfo>> GetAssetInfosAsync(IEnumerable<Guid> assetId)
        {
            return checkAsset(assetId);
        }
    }
}
