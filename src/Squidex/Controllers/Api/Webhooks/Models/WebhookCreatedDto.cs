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
    public class WebhookCreatedDto
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
        /// The id of the schema.
        /// </summary>
        public string SchemaId { get; set; }
    }
}
