// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Immutable;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Translations;

#pragma warning disable MA0048 // File name must match type name

namespace Squidex.Domain.Apps.Core.ValidateContent.Validators;

public delegate Task<IReadOnlyList<Asset>> CheckAssets(IEnumerable<DomainId> ids,
    CancellationToken ct);

public sealed class AssetsValidator : IValidator
{
    private readonly AssetsFieldProperties properties;
    private readonly CollectionValidator? collectionValidator;
    private readonly UniqueValuesValidator<DomainId>? uniqueValidator;
    private readonly CheckAssets checkAssets;

    public AssetsValidator(bool isRequired, AssetsFieldProperties properties, CheckAssets checkAssets)
    {
        Guard.NotNull(properties);
        Guard.NotNull(checkAssets);

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

    public void Validate(object? value, ValidationContext context)
    {
        context.Root.AddTask(ct => ValidateCoreAsync(value, context, ct));
    }

    private async Task ValidateCoreAsync(object? value, ValidationContext context,
        CancellationToken ct)
    {
        var foundIds = new List<DomainId>();

        if (value is ICollection<DomainId> { Count: > 0 } assetIds)
        {
            var assets = await checkAssets(assetIds, ct);
            var index = 1;

            foreach (var assetId in assetIds)
            {
                var assetPath = context.Path.Enqueue($"[{index}]");
                var assetFound = assets.FirstOrDefault(x => x.Id == assetId);

                if (assetFound == null)
                {
                    if (context.Action == ValidationAction.Upsert)
                    {
                        context.AddError(assetPath, T.Get("contents.validation.assetNotFound", new { id = assetId }));
                    }

                    continue;
                }

                foundIds.Add(assetFound.Id);

                ValidateCommon(assetFound, assetPath, context);
                ValidateType(assetFound, assetPath, context);

                if (assetFound.Type == AssetType.Image)
                {
                    if (assetFound.Metadata.TryGetNumber(KnownMetadataKeys.PixelWidth, out var w) &&
                        assetFound.Metadata.TryGetNumber(KnownMetadataKeys.PixelHeight, out var h))
                    {
                        ValidateDimensions((int)w, (int)h, assetPath, context);
                    }
                }
                else if (assetFound.Type == AssetType.Video)
                {
                    if (assetFound.Metadata.TryGetNumber(KnownMetadataKeys.VideoWidth, out var w) &&
                        assetFound.Metadata.TryGetNumber(KnownMetadataKeys.VideoHeight, out var h))
                    {
                        ValidateDimensions((int)w, (int)h, assetPath, context);
                    }
                }

                index++;
            }
        }

        if (collectionValidator != null)
        {
            collectionValidator.Validate(foundIds, context);
        }

        if (uniqueValidator != null)
        {
            uniqueValidator.Validate(foundIds, context);
        }
    }

    private void ValidateCommon(Asset asset, ImmutableQueue<string> path, ValidationContext context)
    {
        if (properties.MinSize != null && asset.FileSize < properties.MinSize)
        {
            var min = properties.MinSize.Value.ToReadableSize();

            context.AddError(path, T.Get("contents.validation.minimumSize", new { size = asset.FileSize.ToReadableSize(), min }));
        }

        if (properties.MaxSize != null && asset.FileSize > properties.MaxSize)
        {
            var max = properties.MaxSize.Value.ToReadableSize();

            context.AddError(path, T.Get("contents.validation.maximumSize", new { size = asset.FileSize.ToReadableSize(), max }));
        }

        if (properties.AllowedExtensions is { Count: > 0 } && !properties.AllowedExtensions.Any(x => asset.FileName.EndsWith("." + x, StringComparison.OrdinalIgnoreCase)))
        {
            context.AddError(path, T.Get("contents.validation.extension"));
        }
    }

    private void ValidateType(Asset asset, ImmutableQueue<string> path, ValidationContext context)
    {
        var type = asset.MimeType == "image/svg+xml" ? AssetType.Image : asset.Type;

        if (properties.ExpectedType != null && properties.ExpectedType != type)
        {
            context.AddError(path, T.Get("contents.validation.assetType", new { type = properties.ExpectedType }));
        }
    }

    private void ValidateDimensions(int w, int h, ImmutableQueue<string> path, ValidationContext context)
    {
        var actualRatio = (double)w / h;

        if (properties.MinWidth != null && w < properties.MinWidth)
        {
            context.AddError(path, T.Get("contents.validation.minimumWidth", new { width = w, min = properties.MinWidth }));
        }

        if (properties.MaxWidth != null && w > properties.MaxWidth)
        {
            context.AddError(path, T.Get("contents.validation.maximumWidth", new { width = w, max = properties.MaxWidth }));
        }

        if (properties.MinHeight != null && h < properties.MinHeight)
        {
            context.AddError(path, T.Get("contents.validation.minimumHeight", new { height = h, min = properties.MinHeight }));
        }

        if (properties.MaxHeight != null && h > properties.MaxHeight)
        {
            context.AddError(path, T.Get("contents.validation.maximumHeight", new { height = h, max = properties.MaxHeight }));
        }

        if (properties.AspectHeight != null && properties.AspectWidth != null)
        {
            var expectedRatio = (double)properties.AspectWidth.Value / properties.AspectHeight.Value;

            if (Math.Abs(expectedRatio - actualRatio) > double.Epsilon)
            {
                context.AddError(path, T.Get("contents.validation.aspectRatio", new { width = properties.AspectWidth, height = properties.AspectHeight }));
            }
        }
    }
}
