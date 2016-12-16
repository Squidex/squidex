// ==========================================================================
//  HistoryEventDto.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.ComponentModel.DataAnnotations;

namespace Squidex.Controllers.Api.History.Models
{
    public class HistoryEventDto
    {
        /// <summary>
        /// The message of the event.
        /// </summary>
        [Required]
        public string Message { get; set; }

        /// <summary>
        /// The user who called the action.
        /// </summary>
        [Required]
        public string Actor { get; set; }

        /// <summary>
        /// Gets a unique id for the event.
        /// </summary>
        public Guid EventId { get; set; }

        /// <summary>
        /// The time when the event happened.
        /// </summary>
        public DateTime Created { get; set; }
    }
}
