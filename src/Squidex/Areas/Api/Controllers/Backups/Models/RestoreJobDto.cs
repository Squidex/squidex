// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.ComponentModel.DataAnnotations;
using NodaTime;
using Squidex.Domain.Apps.Entities.Backup;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Areas.Api.Controllers.Backups.Models
{
    public sealed class RestoreJobDto
    {
        /// <summary>
        /// The uri to load from.
        /// </summary>
        [Required]
        public Uri Uri { get; set; }

        /// <summary>
        /// The status text.
        /// </summary>
        [Required]
        public string Status { get; set; }

        /// <summary>
        /// Indicates when the restore operation has been started.
        /// </summary>
        public Instant Started { get; set; }

        /// <summary>
        /// Indicates if the restore has failed.
        /// </summary>
        public bool IsFailed { get; set; }

        public static RestoreJobDto FromJob(IRestoreJob job)
        {
            return SimpleMapper.Map(job, new RestoreJobDto());
        }
    }
}
