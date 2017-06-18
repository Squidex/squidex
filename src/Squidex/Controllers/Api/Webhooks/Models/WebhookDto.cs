// ==========================================================================
//  WebhookDto.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Squidex.Controllers.Api.Webhooks.Models
{
    public class WebhookDto
    {
        /// <summary>
        /// The id of the webhook.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The id of the schema.
        /// </summary>
        public Guid SchemaId { get; set; }

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
        /// The average request time in milliseconds.
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
        /// The last invokation dumps.
        /// </summary>
        [Required]
        public List<string> LastDumps { get; set; }
    }
}
