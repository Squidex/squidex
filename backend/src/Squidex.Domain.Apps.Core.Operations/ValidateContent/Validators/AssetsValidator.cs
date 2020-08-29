// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
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
        private readonly CheckAssets checkAssets;

        public AssetsValidator(AssetsFieldProperties properties, CheckAssets checkAssets)
        {
            Guard.NotNull(properties, nameof(properties));
            Guard.NotNull(checkAssets, nameof(checkAssets));

            this.properties = properties;

            this.checkAssets = checkAssets;
        }

        public async Task ValidateAsync(object? value, ValidationContext context, AddError addError)
        {
            if (context.Mode == ValidationMode.Optimized)
            {
                return;
            }

            if (value is ICollection<DomainId> assetIds && assetIds.Count > 0)
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
                        addError(path, T.Get("contents.validation.assetNotFound", new { id = assetId }));
                        continue;
                    }

                    if (properties.MinSize.HasValue && asset.FileSize < properties.MinSize)
                    {
                        addError(path, T.Get("contents.validation.minimumSize", new { size = asset.FileSize.ToReadableSize(), min = properties.MinSize.Value.ToReadableSize() }));
                    }

                    if (properties.MaxSize.HasValue && asset.FileSize > properties.MaxSize)
                    {
                        addError(path, T.Get("contents.validation.maximumSize", new { size = asset.FileSize.ToReadableSize(), max = properties.MaxSize.Value.ToReadableSize() }));
                    }

                    if (properties.AllowedExtensions != null &&
                        properties.AllowedExtensions.Count > 0 &&
                       !properties.AllowedExtensions.Any(x => asset.FileName.EndsWith("." + x, StringComparison.OrdinalIgnoreCase)))
                    {
                        addError(path, T.Get("contents.validation.extension"));
                    }

                    if (asset.Type != AssetType.Image)
                    {
                        if (properties.MustBeImage)
                        {
                            addError(path, T.Get("contents.validation.image"));
                        }

                        continue;
                    }

                    var pixelWidth = asset.Metadata.GetPixelWidth();
                    var pixelHeight = asset.Metadata.GetPixelHeight();

                    if (pixelWidth.HasValue && pixelHeight.HasValue)
                    {
                        var w = pixelWidth.Value;
                        var h = pixelHeight.Value;

                        var actualRatio = (double)w / h;

                        if (properties.MinWidth.HasValue && w < properties.MinWidth)
                        {
                            addError(path, T.Get("contents.validation.minimumWidth", new { width = w, min = properties.MinWidth }));
                        }

                        if (properties.MaxWidth.HasValue && w > properties.MaxWidth)
                        {
                            addError(path, T.Get("contents.validation.maximumWidth", new { width = w, max = properties.MaxWidth }));
                        }

                        if (properties.MinHeight.HasValue && h < properties.MinHeight)
                        {
                            addError(path, T.Get("contents.validation.minimumHeight", new { height = h, min = properties.MinHeight }));
                        }

                        if (properties.MaxHeight.HasValue && h > properties.MaxHeight)
                        {
                            addError(path, T.Get("contents.validation.maximumHeight", new { height = h, max = properties.MaxHeight }));
                        }

                        if (properties.AspectHeight.HasValue && properties.AspectWidth.HasValue)
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
    }
}
