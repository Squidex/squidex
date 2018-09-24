// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using Squidex.Infrastructure;
using Squidex.Pipeline;

namespace Squidex.Areas.Api.Controllers.Contents.Models
{
    public sealed class ContentsDto : IGenerateEtag
    {
        /// <summary>
        /// The total number of content items.
        /// </summary>
        public long Total { get; set; }

        /// <summary>
        /// The content items.
        /// </summary>
        public ContentDto[] Items { get; set; }

        public string GenerateETag()
        {
            return string.Join(";", Items?.Select(x => x.GenerateETag()) ?? Enumerable.Empty<string>()).Sha256Base64();
        }
    }
}
