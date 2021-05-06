﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Immutable;
using Squidex.Infrastructure.Validation;

namespace Squidex.Areas.Api.Controllers.Schemas.Models
{
    public sealed class SchemaPropertiesDto
    {
        /// <summary>
        /// Optional label for the editor.
        /// </summary>
        [LocalizedStringLength(100)]
        public string? Label { get; set; }

        /// <summary>
        /// Hints to describe the schema.
        /// </summary>
        [LocalizedStringLength(1000)]
        public string? Hints { get; set; }

        /// <summary>
        /// The url to a the sidebar plugin for content lists.
        /// </summary>
        public string? ContentsSidebarUrl { get; set; }

        /// <summary>
        /// The url to a the sidebar plugin for content items.
        /// </summary>
        public string? ContentSidebarUrl { get; set; }

        /// <summary>
        /// The url to the editor plugin.
        /// </summary>
        public string? ContentEditorUrl { get; set; }

        /// <summary>
        /// True to validate the content items on publish.
        /// </summary>
        public bool ValidateOnPublish { get; set; }

        /// <summary>
        /// Tags for automation processes.
        /// </summary>
        public ImmutableList<string>? Tags { get; set; }
    }
}
