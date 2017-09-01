// ==========================================================================
//  WebhookSchemaDto.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;

namespace Squidex.Controllers.Api.Webhooks.Models
{
    public sealed class WebhookSchemaDto
    {
        /// <summary>
        /// The id of the schema.
        /// </summary>
        public Guid SchemaId { get; set; }

        /// <summary>
        /// True, when to send a message for created events.
        /// </summary>
        public bool SendCreate { get; set; }

        /// <summary>
        /// True, when to send a message for updated events.
        /// </summary>
        public bool SendUpdate { get; set; }

        /// <summary>
        /// True, when to send a message for deleted events.
        /// </summary>
        public bool SendDelete { get; set; }

        /// <summary>
        /// True, when to send a message for published events.
        /// </summary>
        public bool SendPublish { get; set; }

        /// <summary>
        /// True, when to send a message for unpublished events.
        /// </summary>
        public bool SendUnpublish { get; set; }
    }
}
