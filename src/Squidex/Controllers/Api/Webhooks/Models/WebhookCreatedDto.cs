// ==========================================================================
//  WebhookCreatedDto.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.ComponentModel.DataAnnotations;

namespace Squidex.Controllers.Api.Webhooks.Models
{
    public sealed class WebhookCreatedDto
    {
        /// <summary>
        /// The id of the webhook.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The shared secret that is used to calculate the signature.
        /// </summary>
        [Required]
        public string SharedSecret { get; set; }

        /// <summary>
        /// The version of the schema.
        /// </summary>
        public long Version { get; set; }
    }
}
