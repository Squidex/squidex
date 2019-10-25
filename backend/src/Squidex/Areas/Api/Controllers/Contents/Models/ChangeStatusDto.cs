// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.ComponentModel.DataAnnotations;
using NodaTime;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Contents.Commands;

namespace Squidex.Areas.Api.Controllers.Contents.Models
{
    public sealed class ChangeStatusDto
    {
        /// <summary>
        /// The new status.
        /// </summary>
        [Required]
        public Status Status { get; set; }

        /// <summary>
        /// The due time.
        /// </summary>
        public Instant? DueTime { get; set; }

        public ChangeContentStatus ToCommand(Guid id)
        {
            return new ChangeContentStatus { ContentId = id, Status = Status, DueTime = DueTime };
        }
    }
}
