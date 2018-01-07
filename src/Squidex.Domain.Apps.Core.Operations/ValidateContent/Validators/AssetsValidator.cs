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

        public async Task ValidateAsync(object value, ValidationContext context, Action<string> addError)
        {
            if (value is ICollection<Guid> assetIds)
            {
                var assets = await context.GetAssetInfosAsync(assetIds);
                var i = 0;

                foreach (var assetId in assetIds)
                {
                    i++;

                    var asset = assets.FirstOrDefault(x => x.AssetId == assetId);

                    void Error(string message)
                    {
                        addError($"<FIELD> has invalid asset #{i}: {message}");
                    }

                    if (asset == null)
                    {
                        Error($"Id '{assetId}' not found.");
                        continue;
                    }

                    if (properties.MinSize.HasValue && asset.FileSize < properties.MinSize)
                    {
                        Error($"'{asset.FileSize.ToReadableSize()}' less than minimum of '{properties.MinSize.Value.ToReadableSize()}'.");
                    }

                    if (properties.MaxSize.HasValue && asset.FileSize > properties.MaxSize)
                    {
                        Error($"'{asset.FileSize.ToReadableSize()}' greater than maximum of '{properties.MaxSize.Value.ToReadableSize()}'.");
                    }

                    if (properties.AllowedExtensions != null &&
                        properties.AllowedExtensions.Count > 0 &&
                       !properties.AllowedExtensions.Any(x => asset.FileName.EndsWith("." + x, StringComparison.OrdinalIgnoreCase)))
                    {
                        Error("Invalid file extension.");
                    }

                    if (!asset.IsImage)
                    {
                        if (properties.MustBeImage)
                        {
                            Error("Not an image.");
                        }

                        continue;
                    }

                    if (asset.PixelWidth.HasValue &&
                        asset.PixelHeight.HasValue)
                    {
                        var w = asset.PixelWidth.Value;
                        var h = asset.PixelHeight.Value;

                        var actualRatio = (double)w / h;

                        if (properties.MinWidth.HasValue && w < properties.MinWidth)
                        {
                            Error($"Width '{w}px' less than minimum of '{properties.MinWidth}px'.");
                        }

                        if (properties.MaxWidth.HasValue && w > properties.MaxWidth)
                        {
                            Error($"Width '{w}px' greater than maximum of '{properties.MaxWidth}px'.");
                        }

                        if (properties.MinHeight.HasValue && h < properties.MinHeight)
                        {
                            Error($"Height '{h}px' less than minimum of '{properties.MinHeight}px'.");
                        }

                        if (properties.MaxHeight.HasValue && h > properties.MaxHeight)
                        {
                            Error($"Height '{h}px' greater than maximum of '{properties.MaxHeight}px'.");
                        }

                        if (properties.AspectHeight.HasValue && properties.AspectWidth.HasValue)
                        {
                            var expectedRatio = (double)properties.AspectWidth.Value / properties.AspectHeight.Value;

                            if (Math.Abs(expectedRatio - actualRatio) > double.Epsilon)
                            {
                                Error($"Aspect ratio not '{properties.AspectWidth}:{properties.AspectHeight}'.");
                            }
                        }
                    }
                }
            }
        }
    }
}
