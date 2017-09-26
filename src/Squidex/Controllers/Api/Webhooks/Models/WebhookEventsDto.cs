// ==========================================================================
//  WebhookEventsDto.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

namespace Squidex.Controllers.Api.Webhooks.Models
{
    public sealed class WebhookEventsDto
    {
        /// <summary>
        /// The total number of webhook events.
        /// </summary>
        public long Total { get; set; }

        /// <summary>
        /// The webhook events.
        /// </summary>
        public WebhookEventDto[] Items { get; set; }
    }
}
