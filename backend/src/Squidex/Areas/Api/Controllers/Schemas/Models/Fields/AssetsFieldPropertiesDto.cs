// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure.Collections;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Areas.Api.Controllers.Schemas.Models.Fields
{
    public sealed class AssetsFieldPropertiesDto : FieldPropertiesDto
    {
        /// <summary>
        /// The preview mode for the asset.
        /// </summary>
        public AssetPreviewMode PreviewMode { get; set; }

        /// <summary>
        /// The language specific default value as a list of asset ids.
        /// </summary>
        public LocalizedValue<ImmutableList<string>?> DefaultValues { get; set; }

        /// <summary>
        /// The default value as a list of asset ids.
        /// </summary>
        public ImmutableList<string>? DefaultValue { get; set; }

        /// <summary>
        /// The initial id to the folder.
        /// </summary>
        public string? FolderId { get; set; }

        /// <summary>
        /// The minimum allowed items for the field value.
        /// </summary>
        public int? MinItems { get; set; }

        /// <summary>
        /// The maximum allowed items for the field value.
        /// </summary>
        public int? MaxItems { get; set; }

        /// <summary>
        /// The minimum file size in bytes.
        /// </summary>
        public int? MinSize { get; set; }

        /// <summary>
        /// The maximum file size in bytes.
        /// </summary>
        public int? MaxSize { get; set; }

        /// <summary>
        /// The minimum image width in pixels.
        /// </summary>
        public int? MinWidth { get; set; }

        /// <summary>
        /// The maximum image width in pixels.
        /// </summary>
        public int? MaxWidth { get; set; }

        /// <summary>
        /// The minimum image height in pixels.
        /// </summary>
        public int? MinHeight { get; set; }

        /// <summary>
        /// The maximum image height in pixels.
        /// </summary>
        public int? MaxHeight { get; set; }

        /// <summary>
        /// The image aspect width in pixels.
        /// </summary>
        public int? AspectWidth { get; set; }

        /// <summary>
        /// The image aspect height in pixels.
        /// </summary>
        public int? AspectHeight { get; set; }

        /// <summary>
        /// The expected type.
        /// </summary>
        public AssetType? ExpectedType { get; set; }

        /// <summary>
        /// True to resolve first asset in the content list.
        /// </summary>
        public bool ResolveFirst { get; set; }

        /// <summary>
        /// True to resolve first image in the content list.
        /// </summary>
        [Obsolete("Use 'expectedType' field now")]
        public bool MustBeImage
        {
            get => ExpectedType == AssetType.Image;
            set => ExpectedType = value ? AssetType.Image : ExpectedType;
        }

        /// <summary>
        /// True to resolve first image in the content list.
        /// </summary>
        [Obsolete("Use 'resolveFirst' field now")]
        public bool ResolveImage
        {
            get => ResolveFirst;
            set => ResolveFirst = value;
        }

        /// <summary>
        /// The allowed file extensions.
        /// </summary>
        public ImmutableList<string>? AllowedExtensions { get; set; }

        /// <summary>
        /// True, if duplicate values are allowed.
        /// </summary>
        public bool AllowDuplicates { get; set; }

        public override FieldProperties ToProperties()
        {
            var result = SimpleMapper.Map(this, new AssetsFieldProperties());

            return result;
        }
    }
}
