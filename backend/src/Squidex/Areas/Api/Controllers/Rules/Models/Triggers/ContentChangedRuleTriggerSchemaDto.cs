﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Squidex.Areas.Api.Controllers.Rules.Models.Triggers
{
    public sealed class ContentChangedRuleTriggerSchemaDto
    {
        /// <summary>
        /// The id of the schema.
        /// </summary>
        public Guid SchemaId { get; set; }

        /// <summary>
        /// Javascript condition when to trigger.
        /// </summary>
        public string Condition { get; set; }
    }
}
