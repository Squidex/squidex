// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Areas.Api.Controllers.Rules.Models.Triggers
{
    public sealed class ContentChangedRuleTriggerSchemaDto
    {
        /// <summary>
        /// The id of the schema.
        /// </summary>
        public DomainId SchemaId { get; set; }

        /// <summary>
        /// Javascript condition when to trigger.
        /// </summary>
        public string? Condition { get; set; }
    }
}
