﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
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
        public Uri Url { get; set; }

        /// <summary>
        /// The status log.
        /// </summary>
        [Required]
        public List<string> Log { get; set; }

        /// <summary>
        /// The time when the job has been started.
        /// </summary>
        public Instant Started { get; set; }

        /// <summary>
        /// The time when the job has been stopped.
        /// </summary>
        public Instant? Stopped { get; set; }

        /// <summary>
        /// The status of the operation.
        /// </summary>
        public JobStatus Status { get; set; }

        public static RestoreJobDto FromJob(IRestoreJob job)
        {
            return SimpleMapper.Map(job, new RestoreJobDto());
        }
    }
}
