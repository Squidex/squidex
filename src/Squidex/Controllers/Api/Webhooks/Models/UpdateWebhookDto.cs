// ==========================================================================
//  UpdateWebhookDto.cs
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
    public class UpdateWebhookDto
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
