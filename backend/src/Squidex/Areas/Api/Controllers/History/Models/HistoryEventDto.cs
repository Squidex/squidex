﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using NodaTime;
using Squidex.Domain.Apps.Entities.History;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Areas.Api.Controllers.History.Models
{
    public sealed class HistoryEventDto
    {
        /// <summary>
        /// The message for the event.
        /// </summary>
        [Required]
        public string Message { get; set; }

        /// <summary>
        /// The type of the original event.
        /// </summary>
        [Required]
        public string EventType { get; set; }

        /// <summary>
        /// The user who called the action.
        /// </summary>
        [Required]
        public string Actor { get; set; }

        /// <summary>
        /// Gets a unique id for the event.
        /// </summary>
        public DomainId EventId { get; set; }

        /// <summary>
        /// The time when the event happened.
        /// </summary>
        public Instant Created { get; set; }

        /// <summary>
        /// The version identifier.
        /// </summary>
        public long Version { get; set; }

        public static HistoryEventDto FromHistoryEvent(ParsedHistoryEvent historyEvent)
        {
            return SimpleMapper.Map(historyEvent, new HistoryEventDto { EventId = historyEvent.Id });
        }
    }
}
