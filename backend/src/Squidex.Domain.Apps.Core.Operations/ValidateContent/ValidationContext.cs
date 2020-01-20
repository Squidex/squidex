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
    public delegate Task<IReadOnlyList<(Guid SchemaId, Guid Id)>> CheckContents(Guid schemaId, FilterNode<ClrValue> filter);

    public delegate Task<IReadOnlyList<(Guid SchemaId, Guid Id)>> CheckContentsByIds(HashSet<Guid> ids);

    public delegate Task<IReadOnlyList<IAssetInfo>> CheckAssets(IEnumerable<Guid> ids);

    public sealed class ValidationContext
    {
        private readonly Guid contentId;
        private readonly Guid schemaId;
        private readonly CheckContents checkContent;
        private readonly CheckContentsByIds checkContentByIds;
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

        public ValidationMode Mode { get; }

        public ValidationContext(
            Guid contentId,
            Guid schemaId,
            CheckContents checkContent,
            CheckContentsByIds checkContentsByIds,
            CheckAssets checkAsset,
            ValidationMode mode = ValidationMode.Default)
            : this(contentId, schemaId, checkContent, checkContentsByIds, checkAsset, ImmutableQueue<string>.Empty, false, mode)
        {
        }

        private ValidationContext(
            Guid contentId,
            Guid schemaId,
            CheckContents checkContent,
            CheckContentsByIds checkContentByIds,
            CheckAssets checkAsset,
            ImmutableQueue<string> propertyPath,
            bool isOptional,
            ValidationMode mode = ValidationMode.Default)
        {
            Guard.NotNull(checkAsset);
            Guard.NotNull(checkContent);
            Guard.NotNull(checkContentByIds);

            this.propertyPath = propertyPath;

            this.checkContent = checkContent;
            this.checkContentByIds = checkContentByIds;
            this.checkAsset = checkAsset;
            this.contentId = contentId;

            this.schemaId = schemaId;

            Mode = mode;

            IsOptional = isOptional;
        }

        public ValidationContext Optimized(bool isOptimized = true)
        {
            var mode = isOptimized ? ValidationMode.Optimized : ValidationMode.Default;

            if (Mode == mode)
            {
                return this;
            }

            return Clone(propertyPath, IsOptional, mode);
        }

        public ValidationContext Optional(bool isOptional)
        {
            if (IsOptional == isOptional)
            {
                return this;
            }

            return Clone(propertyPath, isOptional, Mode);
        }

        public ValidationContext Nested(string property)
        {
            return Clone(propertyPath.Enqueue(property), IsOptional, Mode);
        }

        private ValidationContext Clone(ImmutableQueue<string> path, bool isOptional, ValidationMode mode)
        {
            return new ValidationContext(
                contentId,
                schemaId,
                checkContent,
                checkContentByIds,
                checkAsset,
                path, isOptional, mode);
        }

        public Task<IReadOnlyList<(Guid SchemaId, Guid Id)>> GetContentIdsAsync(HashSet<Guid> ids)
        {
            return checkContentByIds(ids);
        }

        public Task<IReadOnlyList<(Guid SchemaId, Guid Id)>> GetContentIdsAsync(Guid schemaId, FilterNode<ClrValue> filter)
        {
            return checkContent(schemaId, filter);
        }

        public Task<IReadOnlyList<IAssetInfo>> GetAssetInfosAsync(IEnumerable<Guid> assetId)
        {
            return checkAsset(assetId);
        }
    }
}
