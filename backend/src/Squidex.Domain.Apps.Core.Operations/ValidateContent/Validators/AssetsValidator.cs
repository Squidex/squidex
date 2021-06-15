// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Translations;

namespace Squidex.Domain.Apps.Core.ValidateContent.Validators
{
    public delegate Task<IReadOnlyList<IAssetInfo>> CheckAssets(IEnumerable<DomainId> ids);

    public sealed class AssetsValidator : IValidator
    {
        private readonly AssetsFieldProperties properties;
        private readonly CollectionValidator? collectionValidator;
        private readonly UniqueValuesValidator<DomainId>? uniqueValidator;
        private readonly CheckAssets checkAssets;

        public AssetsValidator(bool isRequired, AssetsFieldProperties properties, CheckAssets checkAssets)
        {
            Guard.NotNull(properties, nameof(properties));
            Guard.NotNull(checkAssets, nameof(checkAssets));

            this.properties = properties;

            if (isRequired || properties.MinItems != null || properties.MaxItems != null)
            {
                collectionValidator = new CollectionValidator(isRequired, properties.MinItems, properties.MaxItems);
            }

            if (!properties.AllowDuplicates)
            {
                uniqueValidator = new UniqueValuesValidator<DomainId>();
            }

            this.checkAssets = checkAssets;
        }

        public async Task ValidateAsync(object? value, ValidationContext context, AddError addError)
        {
            var foundIds = new List<DomainId>();

            if (value is ICollection<DomainId> { Count: > 0 } assetIds)
            {
                var assets = await checkAssets(assetIds);
                var index = 0;

                foreach (var assetId in assetIds)
                {
                    index++;

                    var path = context.Path.Enqueue($"[{index}]");

                    var asset = assets.FirstOrDefault(x => x.AssetId == assetId);

                    if (asset == null)
                    {
                        if (context.Action == ValidationAction.Upsert)
                        {
                            addError(path, T.Get("contents.validation.assetNotFound", new { id = assetId }));
                        }

                        continue;
                    }

                    foundIds.Add(asset.AssetId);

                    ValidateCommon(asset, path, addError);
                    ValidateIsImage(asset, path, addError);

                    if (asset.Type == AssetType.Image)
                    {
                        ValidateImage(asset, path, addError);
                    }
                }
            }

            if (collectionValidator != null)
            {
                await collectionValidator.ValidateAsync(foundIds, context, addError);
            }

            if (uniqueValidator != null)
            {
                await uniqueValidator.ValidateAsync(foundIds, context, addError);
            }
        }

        private void ValidateCommon(IAssetInfo asset, ImmutableQueue<string> path, AddError addError)
        {
            if (properties.MinSize != null && asset.FileSize < properties.MinSize)
            {
                var min = properties.MinSize.Value.ToReadableSize();

                addError(path, T.Get("contents.validation.minimumSize", new { size = asset.FileSize.ToReadableSize(), min }));
            }

            if (properties.MaxSize != null && asset.FileSize > properties.MaxSize)
            {
                var max = properties.MaxSize.Value.ToReadableSize();

                addError(path, T.Get("contents.validation.maximumSize", new { size = asset.FileSize.ToReadableSize(), max }));
            }

            if (properties.AllowedExtensions != null &&
                properties.AllowedExtensions.Count > 0 &&
               !properties.AllowedExtensions.Any(x => asset.FileName.EndsWith("." + x, StringComparison.OrdinalIgnoreCase)))
            {
                addError(path, T.Get("contents.validation.extension"));
            }
        }

        private void ValidateIsImage(IAssetInfo asset, ImmutableQueue<string> path, AddError addError)
        {
            if (properties.MustBeImage && asset.Type != AssetType.Image && asset.MimeType != "image/svg+xml")
            {
                addError(path, T.Get("contents.validation.image"));
            }
        }

        private void ValidateImage(IAssetInfo asset, ImmutableQueue<string> path, AddError addError)
        {
            var pixelWidth = asset.Metadata.GetPixelWidth();
            var pixelHeight = asset.Metadata.GetPixelHeight();

            if (pixelWidth != null && pixelHeight != null)
            {
                var w = pixelWidth.Value;
                var h = pixelHeight.Value;

                var actualRatio = (double)w / h;

                if (properties.MinWidth != null && w < properties.MinWidth)
                {
                    addError(path, T.Get("contents.validation.minimumWidth", new { width = w, min = properties.MinWidth }));
                }

                if (properties.MaxWidth != null && w > properties.MaxWidth)
                {
                    addError(path, T.Get("contents.validation.maximumWidth", new { width = w, max = properties.MaxWidth }));
                }

                if (properties.MinHeight != null && h < properties.MinHeight)
                {
                    addError(path, T.Get("contents.validation.minimumHeight", new { height = h, min = properties.MinHeight }));
                }

                if (properties.MaxHeight != null && h > properties.MaxHeight)
                {
                    addError(path, T.Get("contents.validation.maximumHeight", new { height = h, max = properties.MaxHeight }));
                }

                if (properties.AspectHeight != null && properties.AspectWidth != null)
                {
                    var expectedRatio = (double)properties.AspectWidth.Value / properties.AspectHeight.Value;

                    if (Math.Abs(expectedRatio - actualRatio) > double.Epsilon)
                    {
                        addError(path, T.Get("contents.validation.aspectRatio", new { width = properties.AspectWidth, height = properties.AspectHeight }));
                    }
                }
            }
        }
    }
}
