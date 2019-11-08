// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Areas.Api.Controllers.Statistics.Models
{
    public sealed class LogDownloadDto
    {
        /// <summary>
        /// The url to download the log.
        /// </summary>
        public string? DownloadUrl { get; set; }
    }
}
