// ==========================================================================
//  CreateWebhookDto.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

// ReSharper disable CollectionNeverUpdated.Global

namespace Squidex.Controllers.Api.Webhooks.Models
{
    public class CreateWebhookDto
    {
        /// <summary>
        /// The url of the webhook.
        /// </summary>
        [Required]
        public Uri Url { get; set; }

        /// <summary>
        /// The schema settings.
        /// </summary>
        [Required]
        public List<WebhookSchemaDto> Schemas { get; set; }
    }
}
