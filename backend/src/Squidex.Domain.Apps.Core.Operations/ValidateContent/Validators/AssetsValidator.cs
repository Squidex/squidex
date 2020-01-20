﻿// ==========================================================================
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

namespace Squidex.Domain.Apps.Core.ValidateContent.Validators
{
    public sealed class AssetsValidator : IValidator
    {
        private readonly AssetsFieldProperties properties;

        public AssetsValidator(AssetsFieldProperties properties)
        {
            this.properties = properties;
        }

        public async Task ValidateAsync(object? value, ValidationContext context, AddError addError)
        {
            if (context.Mode == ValidationMode.Optimized)
            {
                return;
            }

            if (value is ICollection<Guid> assetIds && assetIds.Count > 0)
            {
                var assets = await context.GetAssetInfosAsync(assetIds);
                var index = 0;

                foreach (var assetId in assetIds)
                {
                    index++;

                    var path = context.Path.Enqueue($"[{index}]");

                    var asset = assets.FirstOrDefault(x => x.AssetId == assetId);

                    if (asset == null)
                    {
                        addError(path, $"Id '{assetId}' not found.");
                        continue;
                    }

                    if (properties.MinSize.HasValue && asset.FileSize < properties.MinSize)
                    {
                        addError(path, $"'{asset.FileSize.ToReadableSize()}' less than minimum of '{properties.MinSize.Value.ToReadableSize()}'.");
                    }

                    if (properties.MaxSize.HasValue && asset.FileSize > properties.MaxSize)
                    {
                        addError(path, $"'{asset.FileSize.ToReadableSize()}' greater than maximum of '{properties.MaxSize.Value.ToReadableSize()}'.");
                    }

                    if (properties.AllowedExtensions != null &&
                        properties.AllowedExtensions.Count > 0 &&
                       !properties.AllowedExtensions.Any(x => asset.FileName.EndsWith("." + x, StringComparison.OrdinalIgnoreCase)))
                    {
                        addError(path, "Invalid file extension.");
                    }

                    if (asset.Type != AssetType.Image)
                    {
                        if (properties.MustBeImage)
                        {
                            addError(path, "Not an image.");
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
                            addError(path, $"Width '{w}px' less than minimum of '{properties.MinWidth}px'.");
                        }

                        if (properties.MaxWidth.HasValue && w > properties.MaxWidth)
                        {
                            addError(path, $"Width '{w}px' greater than maximum of '{properties.MaxWidth}px'.");
                        }

                        if (properties.MinHeight.HasValue && h < properties.MinHeight)
                        {
                            addError(path, $"Height '{h}px' less than minimum of '{properties.MinHeight}px'.");
                        }

                        if (properties.MaxHeight.HasValue && h > properties.MaxHeight)
                        {
                            addError(path, $"Height '{h}px' greater than maximum of '{properties.MaxHeight}px'.");
                        }

                        if (properties.AspectHeight.HasValue && properties.AspectWidth.HasValue)
                        {
                            var expectedRatio = (double)properties.AspectWidth.Value / properties.AspectHeight.Value;

                            if (Math.Abs(expectedRatio - actualRatio) > double.Epsilon)
                            {
                                addError(path, $"Aspect ratio not '{properties.AspectWidth}:{properties.AspectHeight}'.");
                            }
                        }
                    }
                }
            }
        }
    }
}
