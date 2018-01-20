// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Immutable;
using NJsonSchema.Annotations;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Areas.Api.Controllers.Schemas.Models.Fields
{
    [JsonSchema("Assets")]
    public sealed class AssetsFieldPropertiesDto : FieldPropertiesDto
    {
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
        /// Defines if the asset must be an image.
        /// </summary>
        public bool MustBeImage { get; set; }

        /// <summary>
        /// The allowed file extensions.
        /// </summary>
        public string[] AllowedExtensions { get; set; }

        public override FieldProperties ToProperties()
        {
            var result = SimpleMapper.Map(this, new AssetsFieldProperties());

            if (AllowedExtensions != null)
            {
                result.AllowedExtensions = ImmutableList.Create(AllowedExtensions);
            }

            return result;
        }
    }
}
