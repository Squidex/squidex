// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using NodaTime;

namespace Squidex.Areas.Api.Controllers.Backups.Models
{
    public sealed class BackupJobDto
    {
        /// <summary>
        /// The id of the backup job.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The time when the job has been started.
        /// </summary>
        public Instant Started { get; set; }

        /// <summary>
        /// The time when the job has been stopped.
        /// </summary>
        public Instant? Stopped { get; set; }

        /// <summary>
        /// The number of handled events.
        /// </summary>
        public int HandledEvents { get; set; }

        /// <summary>
        /// The number of handled assets.
        /// </summary>
        public int HandledAssets { get; set; }

        /// <summary>
        /// Indicates if the job has failed.
        /// </summary>
        public bool IsFailed { get; set; }
    }
}
