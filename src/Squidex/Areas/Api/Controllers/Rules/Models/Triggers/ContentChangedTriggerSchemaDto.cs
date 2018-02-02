// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Squidex.Areas.Api.Controllers.Rules.Models.Triggers
{
    public sealed class ContentChangedTriggerSchemaDto
    {
        /// <summary>
        /// The id of the schema.
        /// </summary>
        public Guid SchemaId { get; set; }

        /// <summary>
        /// Determines whether to handle the event when a content is created.
        /// </summary>
        public bool SendCreate { get; set; }

        /// <summary>
        /// Determines whether to handle the event when a content is updated.
        /// </summary>
        public bool SendUpdate { get; set; }

        /// <summary>
        /// Determines whether to handle the event when a content is deleted.
        /// </summary>
        public bool SendDelete { get; set; }

        /// <summary>
        /// Determines whether to handle the event when a content is published.
        /// </summary>
        public bool SendPublish { get; set; }
    }
}
