// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.ObjectModel;
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
        /// True to validate the content items on publish.
        /// </summary>
        public bool ValidateOnPublish { get; set; }

        /// <summary>
        /// Tags for automation processes.
        /// </summary>
        public ReadOnlyCollection<string>? Tags { get; set; }
    }
}
