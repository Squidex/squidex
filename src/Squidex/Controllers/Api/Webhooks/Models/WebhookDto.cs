// ==========================================================================
//  WebhookDto.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.ComponentModel.DataAnnotations;
using NodaTime;
using Squidex.Infrastructure;
using System.Collections.Generic;

namespace Squidex.Controllers.Api.Webhooks.Models
{
    public sealed class WebhookDto
    {
        /// <summary>
        /// The id of the webhook.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The user that has created the webhook.
        /// </summary>
        [Required]
        public RefToken CreatedBy { get; set; }

        /// <summary>
        /// The user that has updated the webhook.
        /// </summary>
        [Required]
        public RefToken LastModifiedBy { get; set; }

        /// <summary>
        /// The date and time when the webhook has been created.
        /// </summary>
        public Instant Created { get; set; }

        /// <summary>
        /// The date and time when the webhook has been modified last.
        /// </summary>
        public Instant LastModified { get; set; }

        /// <summary>
        /// The version of the webhook.
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// The number of succceeded calls.
        /// </summary>
        public long TotalSucceeded { get; set; }

        /// <summary>
        /// The number of failed calls.
        /// </summary>
        public long TotalFailed { get; set; }

        /// <summary>
        /// The number of timedout calls.
        /// </summary>
        public long TotalTimedout { get; set; }

        /// <summary>
        /// The average response time in milliseconds.
        /// </summary>
        public long AverageRequestTimeMs { get; set; }

        /// <summary>
        /// The url of the webhook.
        /// </summary>
        [Required]
        public Uri Url { get; set; }

        /// <summary>
        /// The shared secret that is used to calculate the signature.
        /// </summary>
        [Required]
        public string SharedSecret { get; set; }

        /// <summary>
        /// The schema settings.
        /// </summary>
        [Required]
        public List<WebhookSchemaDto> Schemas { get; set; }
    }
}
