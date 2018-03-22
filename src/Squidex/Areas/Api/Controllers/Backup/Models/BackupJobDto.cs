// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.ComponentModel.DataAnnotations;
using NodaTime;

namespace Squidex.Areas.Api.Controllers.Backup.Models
{
    public sealed class BackupJobDto
    {
        /// <summary>
        /// The id of the backup job.
        /// </summary>
        [Required]
        public Guid Id { get; set; }

        /// <summary>
        /// The time when the job has been started.
        /// </summary>
        [Required]
        public Instant Started { get; set; }

        /// <summary>
        /// The time when the job has been stopped.
        /// </summary>
        public Instant? Stopped { get; }

        /// <summary>
        /// Indicates if the job has failed.
        /// </summary>
        public bool Failed { get; }
    }
}
