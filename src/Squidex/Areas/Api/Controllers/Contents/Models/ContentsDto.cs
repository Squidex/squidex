// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Areas.Api.Controllers.Contents.Models
{
    public sealed class ContentsDto
    {
        /// <summary>
        /// The total number of content items.
        /// </summary>
        public long Total { get; set; }

        /// <summary>
        /// The content items.
        /// </summary>
        public ContentDto[] Items { get; set; }
    }
}
