// ==========================================================================
//  WebhookEventDto.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.ComponentModel.DataAnnotations;
using NodaTime;
using Squidex.Domain.Apps.Read.Schemas;

namespace Squidex.Controllers.Api.Webhooks.Models
{
    public class WebhookEventDto
    {
        /// <summary>
        /// The request url.
        /// </summary>
        [Required]
        public Uri RequestUrl { get; set; }

        /// <summary>
        /// The name of the event.
        /// </summary>
        [Required]
        public string EventName { get; set; }

        /// <summary>
        /// The webhook event id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The last dump.
        /// </summary>
        public string LastDump { get; set; }

        /// <summary>
        /// The number of calls.
        /// </summary>
        public int NumCalls { get; set; }

        /// <summary>
        /// The next attempt.
        /// </summary>
        public Instant? NextAttempt { get; set; }

        /// <summary>
        /// The result of the event.
        /// </summary>
        public WebhookResult Result { get; set; }

        /// <summary>
        /// The result of the job.
        /// </summary>
        public WebhookJobResult JobResult { get; set; }
    }
}
