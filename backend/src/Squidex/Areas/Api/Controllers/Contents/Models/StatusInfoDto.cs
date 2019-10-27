// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using Squidex.Domain.Apps.Core.Contents;

namespace Squidex.Areas.Api.Controllers.Contents.Models
{
    public sealed class StatusInfoDto
    {
        /// <summary>
        /// The name of the status.
        /// </summary>
        [Required]
        public Status Status { get; set; }

        /// <summary>
        /// The color of the status.
        /// </summary>
        [Required]
        public string Color { get; set; }

        public static StatusInfoDto FromStatusInfo(StatusInfo statusInfo)
        {
            return new StatusInfoDto { Status = statusInfo.Status, Color = statusInfo.Color };
        }
    }
}
