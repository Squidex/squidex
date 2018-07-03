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

        /// <summary>
        /// Determines whether to handle the event when a content is unpublished.
        /// </summary>
        public bool SendUnpublish { get; set; }

        /// <summary>
        /// Determines whether to handle the event when a content is archived.
        /// </summary>
        public bool SendArchived { get; set; }

        /// <summary>
        /// Determines whether to handle the event when a content is restored.
        /// </summary>
        public bool SendRestore { get; set; }
    }
}
