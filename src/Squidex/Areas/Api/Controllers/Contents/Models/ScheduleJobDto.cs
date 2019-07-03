﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.ComponentModel.DataAnnotations;
using NodaTime;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure;

namespace Squidex.Areas.Api.Controllers.Contents.Models
{
    public sealed class ScheduleJobDto
    {
        /// <summary>
        /// The id of the schedule job.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The new status.
        /// </summary>
        public Status Status { get; set; }

        /// <summary>
        /// The target date and time when the content should be scheduled.
        /// </summary>
        public Instant DueTime { get; set; }

        /// <summary>
        /// The user who schedule the content.
        /// </summary>
        [Required]
        public RefToken ScheduledBy { get; set; }
    }
}
