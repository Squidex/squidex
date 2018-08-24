// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;

namespace Squidex.Areas.Api.Controllers.UI.Models
{
    public sealed class UISettingsDto
    {
        /// <summary>
        /// The type of the map control.
        /// </summary>
        [Required]
        public string MapType { get; set; }

        /// <summary>
        /// The key for the map control.
        /// </summary>
        [Required]
        public string MapKey { get; set; }

        /// <summary>
        /// Indicates whether twitter actions are supported.
        /// </summary>
        public bool SupportsTwitterActions { get; set; }
    }
}
